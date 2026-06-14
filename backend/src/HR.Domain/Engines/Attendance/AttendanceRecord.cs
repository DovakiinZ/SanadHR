using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Attendance;

/// <summary>A single day's attendance state for an employee. Written by approved leave /
/// attendance-correction requests (and later by the attendance module itself).</summary>
public class AttendanceRecord : TenantEntity
{
    public Guid EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public AttendanceStatus Status { get; set; }

    /// <summary>Punch in/out times (set by Missing Punch / Attendance Correction requests).</summary>
    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }

    /// <summary>Origin of this record, e.g. "LeaveRequest" / "AttendanceCorrection" / "MissingPunch".</summary>
    public string? Source { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? Notes { get; set; }
}
