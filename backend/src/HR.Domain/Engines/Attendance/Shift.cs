using HR.Domain.Common;

namespace HR.Domain.Engines.Attendance;

/// <summary>A work-shift template. Either fixed (Start/End define scheduled hours and grace windows
/// drive lateness) or flexible (only <see cref="RequiredMinutes"/> matters — check in any time).</summary>
public class Shift : TenantEntity
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;

    /// <summary>Scheduled start/end (time of day). Ignored when <see cref="IsFlexible"/>.</summary>
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    /// <summary>Required working time per day, in minutes (e.g. 480 = 8h).</summary>
    public int RequiredMinutes { get; set; } = 480;

    /// <summary>Unpaid break inside the shift, in minutes.</summary>
    public int BreakMinutes { get; set; }

    // Grace windows (minutes) around the scheduled start/end.
    public int GraceBeforeStartMinutes { get; set; }
    public int GraceAfterStartMinutes { get; set; }
    public int GraceBeforeEndMinutes { get; set; }
    public int GraceAfterEndMinutes { get; set; }

    public bool OvertimeAllowed { get; set; }
    public bool LateDeductionEnabled { get; set; }

    /// <summary>Flexible shift: ignore exact arrival, only total required hours matter.</summary>
    public bool IsFlexible { get; set; }

    /// <summary>Weekend day numbers as CSV of <see cref="System.DayOfWeek"/> ints (Sun=0..Sat=6),
    /// e.g. "5,6" for Friday+Saturday.</summary>
    public string WeekendDays { get; set; } = "5,6";

    public bool IsActive { get; set; } = true;
}
