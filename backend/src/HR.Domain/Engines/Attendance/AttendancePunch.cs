using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Attendance;

/// <summary>A single raw punch (clock in/out) for an employee. Punches are the source of truth for the
/// day's CheckIn/CheckOut; the calculation engine reduces a day's punches into one
/// <see cref="AttendanceRecord"/>.</summary>
public class AttendancePunch : TenantEntity
{
    public Guid EmployeeId { get; set; }

    /// <summary>The day's attendance record this punch rolls up into (set once the record exists).</summary>
    public Guid? AttendanceRecordId { get; set; }

    public DateTime PunchTime { get; set; }
    public PunchDirection Direction { get; set; }

    /// <summary>One of <see cref="AttendanceSources"/>.</summary>
    public string Source { get; set; } = AttendanceSources.ManualEntry;

    // Optional geofence metadata (mobile check-in).
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public string? Notes { get; set; }
}
