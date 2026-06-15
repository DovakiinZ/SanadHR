using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Leave;

/// <summary>The canonical HR-managed record of an approved or assigned leave. Created when a leave
/// request is approved (Source=Request, linked via RequestInstanceId) or when HR assigns leave directly
/// (Source=HRAssignment, linked via LeaveAssignmentId). Stores the balance snapshot at creation so the
/// Leaves page can always show "balance before / after". Cancellation is a state change — records are
/// never hard-deleted.</summary>
public class LeaveRecord : TenantEntity
{
    public string RecordNumber { get; set; } = null!;     // e.g. LV-2026-000123

    public Guid EmployeeId { get; set; }
    public Guid LeaveTypeId { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal DaysCount { get; set; }

    /// <summary>Whether this leave type deducts from the balance. False → "does not affect balance".</summary>
    public bool AffectsBalance { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }

    public LeaveRecordStatus Status { get; set; } = LeaveRecordStatus.Approved;
    public LeaveRecordSource Source { get; set; } = LeaveRecordSource.Request;

    public Guid? RequestInstanceId { get; set; }
    public Guid? LeaveAssignmentId { get; set; }

    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public string? Notes { get; set; }
    public string? AttachmentUrl { get; set; }

    /// <summary>Latest generated "Leave Record" document.</summary>
    public Guid? GeneratedDocumentId { get; set; }

    public DateTime? CanceledAt { get; set; }
    public Guid? CanceledByUserId { get; set; }
    public string? CancelReason { get; set; }
}
