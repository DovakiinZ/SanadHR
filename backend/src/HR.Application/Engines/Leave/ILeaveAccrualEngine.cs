namespace HR.Application.Engines.Leave;

/// <summary>A single row of the leave ledger for the timeline view (accrual or usage), with the
/// continuous running balance recomputed by chronological order.</summary>
public record LeaveLedgerEntryDto(
    DateTime Date,
    string Type,
    decimal Amount,
    decimal RunningBalance,
    string? Reason,
    bool IsUnpaidPeriod);

/// <summary>An unpaid-leave period — rendered as a dotted/grey stem segment on the timeline.</summary>
public record LeaveLedgerGapDto(DateTime Start, DateTime End, decimal Days);

/// <summary>The full ledger view for one employee + leave type.</summary>
public record LeaveLedgerView
{
    public Guid EmployeeId { get; init; }
    public Guid LeaveTypeId { get; init; }
    public decimal CurrentBalance { get; init; }
    public decimal AccruedToDate { get; init; }
    public decimal UsedToDate { get; init; }
    public List<LeaveLedgerEntryDto> Entries { get; init; } = new();
    public List<LeaveLedgerGapDto> UnpaidPeriods { get; init; } = new();
}

/// <summary>Ledger-based leave accrual. Accrual is posted as periodic (monthly) Accrual transactions on
/// the existing LeaveBalanceTransaction ledger; usage/restoration rows written by the leave lifecycle
/// are left untouched. Recalculate rebuilds only the Accrual rows — the trigger for back-dated unpaid
/// leave, which shifts the seniority service end-date.</summary>
public interface ILeaveAccrualEngine
{
    /// <summary>Rebuild the accrual ledger for an employee + leave type from the hire date to today,
    /// excluding unpaid-leave days. Idempotent: deletes prior Accrual rows and re-posts. Returns the
    /// number of accrual entries posted.</summary>
    Task<int> RecalculateAsync(Guid employeeId, Guid leaveTypeId, CancellationToken ct = default);

    /// <summary>Return the ordered ledger (accrual + usage) with a continuous running balance and the
    /// unpaid-leave periods for the timeline.</summary>
    Task<LeaveLedgerView> GetLedgerAsync(Guid employeeId, Guid leaveTypeId, CancellationToken ct = default);
}
