using HR.Application.Engines.Finance;
using HR.Application.Common.Interfaces;
using HR.Application.Engines.Finance.Events;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Engines.Finance;

/// <summary>Processes a single payroll run item: posts one employee's snapshotted payslip components to
/// the immutable ledger, idempotently. Scoped — each concurrent worker resolves its own instance (and its
/// own DbContext) from a fresh DI scope. Idempotency is guaranteed by checking for existing ledger entries
/// referencing the payslip, so a crash-and-resume never double-posts.</summary>
public sealed class PayrollItemExecutor
{
    private const string PayslipReference = PayslipLedgerMapper.PayslipReference;

    private readonly ApplicationDbContext _db;
    private readonly IFinancialLedger _ledger;
    private readonly IDomainEventPublisher _events;

    public PayrollItemExecutor(ApplicationDbContext db, IFinancialLedger ledger, IDomainEventPublisher events)
    {
        _db = db;
        _ledger = ledger;
        _events = events;
    }

    public async Task<bool> ExecuteItemAsync(Guid itemId, CancellationToken ct)
    {
        var item = await _db.PayrollRunItems.FirstOrDefaultAsync(i => i.Id == itemId, ct);
        if (item is null) return false;
        if (item.State == PayrollRunItemState.Completed) return true;

        var payslip = await _db.PayrollPayslips.FirstOrDefaultAsync(p => p.Id == item.PayslipId, ct);
        if (payslip is null)
        {
            item.State = PayrollRunItemState.Failed;
            item.Error = "Payslip snapshot not found.";
            await _db.SaveChangesAsync(ct);
            return false;
        }

        item.State = PayrollRunItemState.Processing;
        item.Attempts += 1;
        item.StartedAt ??= DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        try
        {
            // Idempotency: if entries already reference this payslip, treat as posted (resume-safe).
            var alreadyPosted = payslip.LedgerPosted
                || await _db.FinancialLedgerEntries
                    .AnyAsync(e => e.ReferenceType == PayslipReference && e.ReferenceId == payslip.Id, ct);

            var postings = alreadyPosted ? new List<LedgerPostingRequest>() : PayslipLedgerMapper.Map(item.PayrollRunId, payslip);
            if (!alreadyPosted && postings.Count > 0)
                await _ledger.PostManyAsync(postings, ct);

            payslip.LedgerPosted = true;
            payslip.LedgerPostedAt = DateTime.UtcNow;
            item.LedgerEntryCount = alreadyPosted ? item.LedgerEntryCount : postings.Count;
            // 2C: flip consumed transactions to Posted (idempotent; runs on fresh + resumed executions).
            await PostedTransactionStamper.StampAsync(_db, item.PayrollRunId, payslip.EmployeeId, ct);
            item.State = PayrollRunItemState.Completed;
            item.CompletedAt = DateTime.UtcNow;
            item.Error = null;
            await _db.SaveChangesAsync(ct);

            await _events.PublishAsync(
                new PayslipPostedEvent(item.PayrollRunId, item.EmployeeId, payslip.Id, payslip.NetAmount, item.LedgerEntryCount), ct);
            return true;
        }
        catch (Exception ex)
        {
            item.State = PayrollRunItemState.Failed;
            item.Error = ex.Message;
            await _db.SaveChangesAsync(ct);
            return false;
        }
    }
}
