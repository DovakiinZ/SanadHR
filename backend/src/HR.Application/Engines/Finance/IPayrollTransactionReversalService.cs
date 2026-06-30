namespace HR.Application.Engines.Finance;

public sealed record ReversalResult(Guid ReversedTransactionId, Guid CounterLedgerEntryId, Guid? CorrectionTransactionId);

/// <summary>Reverses a posted payroll transaction (counter ledger entry + Posted→Reversed) and optionally
/// creates a Draft correction that flows into the next run. Posted records are never edited in place.</summary>
public interface IPayrollTransactionReversalService
{
    Task<ReversalResult> ReverseAsync(Guid transactionId, string reason, bool createCorrection,
        decimal? correctedAmount, CancellationToken ct = default);
}
