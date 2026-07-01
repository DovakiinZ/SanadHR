using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Engines.Finance;

/// <summary>Stamps posting metadata onto payroll transactions once their per-record ledger entries exist.
/// Idempotent: only flips still-Approved transactions, so re-running a failed execution converges and a
/// transaction is never double-stamped.</summary>
public static class PostedTransactionStamper
{
    public static async Task StampAsync(ApplicationDbContext db, Guid runId, Guid employeeId, CancellationToken ct)
    {
        var entries = await db.FinancialLedgerEntries
            .Where(e => e.PayrollRunId == runId && e.EmployeeId == employeeId
                        && e.ReferenceType == PayslipLedgerMapper.TransactionReference && e.ReferenceId != null)
            .Select(e => new { e.Id, TxnId = e.ReferenceId!.Value, e.PostedAt })
            .ToListAsync(ct);
        if (entries.Count == 0) return;

        var txnIds = entries.Select(e => e.TxnId).ToList();
        var txns = await db.PayrollTransactions
            .Where(t => txnIds.Contains(t.Id) && t.Status == PayrollTransactionStatus.Approved)
            .ToListAsync(ct);

        foreach (var t in txns)
        {
            var entry = entries.First(e => e.TxnId == t.Id);
            t.Status = PayrollTransactionStatus.Posted;
            t.PayrollRunId = runId;
            t.PostedAt = entry.PostedAt;
            t.LedgerEntryId = entry.Id;
        }
        // Saved by the caller (PayrollItemExecutor) inside the same unit of work.
    }
}
