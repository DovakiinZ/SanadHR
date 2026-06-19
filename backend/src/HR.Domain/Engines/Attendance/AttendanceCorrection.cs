using HR.Domain.Common;

namespace HR.Domain.Engines.Attendance;

/// <summary>An audit-friendly record of an HR/Admin correction (or approved correction request) to a
/// day's check-in/out. Applying a correction recalculates the day's <see cref="AttendanceRecord"/>.</summary>
public class AttendanceCorrection : TenantEntity
{
    public Guid AttendanceRecordId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime Date { get; set; }

    public DateTime? OldCheckIn { get; set; }
    public DateTime? OldCheckOut { get; set; }
    public DateTime? NewCheckIn { get; set; }
    public DateTime? NewCheckOut { get; set; }

    public string? Reason { get; set; }

    /// <summary>Set when the correction came from an approved Attendance-Correction request.</summary>
    public Guid? RequestInstanceId { get; set; }
}
