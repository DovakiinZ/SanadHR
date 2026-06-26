using HR.Domain.Engines.Finance.Entities;
using HR.Domain.Enums;

namespace HR.Application.Engines.Finance;

/// <summary>A request to post a single financial movement to the immutable ledger.</summary>
public record LedgerPostingRequest
{
    public Guid EmployeeId { get; init; }
    public FinanceSourceModule SourceModule { get; init; }
    public string ComponentCode { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "SAR";
    public LedgerDirection Direction { get; init; }
    public string? Description { get; init; }
    public string? ReferenceType { get; init; }
    public Guid? ReferenceId { get; init; }
    public Guid? PayrollRunId { get; init; }
    public DateTime? PostedAt { get; init; }

    /// <summary>Optional caller-supplied entry number. When set it is used verbatim (the caller guarantees
    /// uniqueness — e.g. payroll execution derives a deterministic number per payslip/component so
    /// concurrent workers never collide). When null the ledger auto-generates a sequential number.</summary>
    public string? EntryNumber { get; init; }
}

/// <summary>Filter for querying the ledger.</summary>
public record LedgerQuery
{
    public Guid? EmployeeId { get; init; }
    public Guid? PayrollRunId { get; init; }
    public FinanceSourceModule? SourceModule { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
}

/// <summary>The append-only financial ledger. Posting creates immutable entries; corrections are made by
/// <see cref="ReverseAsync"/>, which writes a counter-entry rather than mutating the original. This is the
/// single system of record for every monetary movement across all modules.</summary>
public interface IFinancialLedger
{
    Task<FinancialLedgerEntry> PostAsync(LedgerPostingRequest request, CancellationToken ct = default);

    /// <summary>Post many entries atomically (single SaveChanges).</summary>
    Task<IReadOnlyList<FinancialLedgerEntry>> PostManyAsync(
        IEnumerable<LedgerPostingRequest> requests, CancellationToken ct = default);

    /// <summary>Reverse an entry by posting an opposite counter-entry. The original is never modified.</summary>
    Task<FinancialLedgerEntry> ReverseAsync(Guid entryId, string reason, CancellationToken ct = default);

    /// <summary>Net balance for an employee in a currency: Σ credits − Σ debits.</summary>
    Task<decimal> GetEmployeeBalanceAsync(Guid employeeId, string currency = "SAR", CancellationToken ct = default);

    Task<IReadOnlyList<FinancialLedgerEntry>> QueryAsync(LedgerQuery query, CancellationToken ct = default);
}
