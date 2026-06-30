using HR.Application.Common.Exceptions;
using HR.Application.Engines.Finance;
using HR.Domain.Engines.Finance.Entities;
using HR.Domain.Engines.Finance.StateMachine;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Engines.Finance;

public sealed class PayrollTransactionReversalService : IPayrollTransactionReversalService
{
    private readonly ApplicationDbContext _db;
    private readonly IFinancialLedger _ledger;

    public PayrollTransactionReversalService(ApplicationDbContext db, IFinancialLedger ledger)
    {
        _db = db;
        _ledger = ledger;
    }

    private static DateTime AsUtc(DateTime d) => d.Kind == DateTimeKind.Utc ? d : DateTime.SpecifyKind(d, DateTimeKind.Utc);

    public async Task<ReversalResult> ReverseAsync(Guid transactionId, string reason, bool createCorrection,
        decimal? correctedAmount, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("A reversal reason is required.");

        var txn = await _db.PayrollTransactions.FirstOrDefaultAsync(t => t.Id == transactionId, ct)
            ?? throw new NotFoundException("PayrollTransaction", transactionId);

        if (txn.Status != PayrollTransactionStatus.Posted)
            throw new DomainException("Only a posted transaction can be reversed.");
        if (txn.LedgerEntryId is not { } ledgerEntryId)
            throw new DomainException("This transaction has no ledger entry to reverse.");

        var previousStatus = txn.Status;
        var counter = await _ledger.ReverseAsync(ledgerEntryId, reason, ct);

        PayrollTransactionStateMachine.EnsureCanTransition(previousStatus, PayrollTransactionStatus.Reversed);
        txn.Status = PayrollTransactionStatus.Reversed;
        txn.ReversalReason = reason;

        Guid? correctionId = null;
        if (createCorrection)
        {
            if (correctedAmount is not { } amount || amount < 0)
                throw new DomainException("A non-negative corrected amount is required to create a correction.");

            var today = AsUtc(DateTime.UtcNow.Date);
            var correction = new PayrollTransaction
            {
                Kind = txn.Kind,
                EmployeeId = txn.EmployeeId,
                TypeId = txn.TypeId,
                Amount = amount,
                EffectiveDate = today,
                TransactionDate = today,
                TargetPeriodYear = today.Year,
                TargetPeriodMonth = today.Month,
                Notes = $"Correction of reversed transaction {txn.Id}",
                SourceModule = "Correction",
                ReversesTransactionId = txn.Id,
                Status = PayrollTransactionStatus.Draft,
            };
            _db.PayrollTransactions.Add(correction);
            await _db.SaveChangesAsync(ct);
            correctionId = correction.Id;
        }
        else
        {
            await _db.SaveChangesAsync(ct);
        }

        return new ReversalResult(txn.Id, counter.Id, correctionId);
    }
}
