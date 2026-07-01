using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Finance.Entities;

/// <summary>One contributing attendance day behind an attendance-sourced PayrollTransaction deduction.
/// Snapshotted at sync time so the breakdown drawer and audit stay accurate even if the underlying
/// attendance record later changes (overview §19 — never overwrite original business information).</summary>
public class PayrollTransactionAttendanceReference : TenantEntity
{
    /// <summary>The attendance deduction record this row explains.</summary>
    public Guid PayrollTransactionId { get; set; }

    /// <summary>The attendance day that produced this share of the deduction.</summary>
    public Guid AttendanceRecordId { get; set; }

    /// <summary>Snapshot of the attendance date.</summary>
    public DateTime Date { get; set; }

    public AttendancePenaltyKind PenaltyKind { get; set; }

    /// <summary>Late/shortage minutes for this day (0 for absence).</summary>
    public int Minutes { get; set; }

    /// <summary>Absent days for this row (1 for an absence day, else 0).</summary>
    public int Days { get; set; }

    /// <summary>This row's share of the deduction amount (pre-rounding; the transaction Amount is authoritative).</summary>
    public decimal AmountContribution { get; set; }
}
