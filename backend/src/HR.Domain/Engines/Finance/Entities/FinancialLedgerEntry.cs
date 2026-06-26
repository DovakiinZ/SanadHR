using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Finance.Entities;

/// <summary>An immutable, append-only financial ledger entry — the atom of the whole financial engine.
/// Every monetary movement from any module (attendance, leave, loan, expense, bonus, GOSI, EOS, manual
/// adjustment, payroll) becomes one of these. Rows are NEVER updated or deleted: a correction is a new
/// <see cref="LedgerEntryStatus.Reversal"/> entry that points back via <see cref="ReversesEntryId"/> and
/// carries the opposite <see cref="Direction"/>, so the pair nets to zero while preserving full history.</summary>
public class FinancialLedgerEntry : TenantEntity
{
    /// <summary>Human-readable reference, e.g. LED-2026-00000123.</summary>
    public string EntryNumber { get; set; } = string.Empty;

    public Guid EmployeeId { get; set; }

    /// <summary>Which business module originated this movement.</summary>
    public FinanceSourceModule SourceModule { get; set; }

    /// <summary>The pay/financial component code, e.g. BASIC, HOUSING, GOSI_EE, LOAN_REPAYMENT.</summary>
    public string ComponentCode { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>Always non-negative; the sign is conveyed by <see cref="Direction"/>.</summary>
    public decimal Amount { get; set; }

    public string Currency { get; set; } = "SAR";

    public LedgerDirection Direction { get; set; }

    /// <summary>Type of the originating record (e.g. "PayrollRun", "Loan", "RequestInstance").</summary>
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }

    /// <summary>Set when the entry was produced by a payroll run.</summary>
    public Guid? PayrollRunId { get; set; }

    public LedgerEntryStatus Status { get; set; } = LedgerEntryStatus.Posted;

    /// <summary>Schema/semantic version of the posting, for forward compatibility.</summary>
    public int Version { get; set; } = 1;

    /// <summary>For a reversal entry, the id of the original entry it reverses.</summary>
    public Guid? ReversesEntryId { get; set; }

    public DateTime PostedAt { get; set; }
    public Guid? ActorUserId { get; set; }

    /// <summary>The amount as a signed value: +Amount for a credit (owed to employee), −Amount for a debit.</summary>
    public decimal SignedAmount => Direction == LedgerDirection.Credit ? Amount : -Amount;
}
