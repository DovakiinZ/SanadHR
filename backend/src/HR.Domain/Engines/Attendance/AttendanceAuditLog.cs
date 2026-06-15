using HR.Domain.Common;

namespace HR.Domain.Engines.Attendance;

/// <summary>Append-only audit trail for attendance changes (manual punch, correction, recalculation,
/// request-driven updates). Shown in the attendance detail drawer.</summary>
public class AttendanceAuditLog : TenantEntity
{
    public Guid? AttendanceRecordId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime Date { get; set; }

    /// <summary>e.g. "ManualPunch", "Correction", "Recalculated", "LeaveApplied".</summary>
    public string Action { get; set; } = null!;

    public string? DetailsAr { get; set; }
    public string? DetailsEn { get; set; }
    public string? DetailsJson { get; set; }

    public Guid? ActorUserId { get; set; }
    public DateTime At { get; set; }
}
