using HR.Domain.Common;

namespace HR.Domain.Engines.Attendance;

/// <summary>Tenant-level attendance defaults applied when a shift does not override them (e.g. default
/// grace, rounding, and whether absence is auto-marked for days with no punches).</summary>
public class AttendancePolicy : TenantEntity
{
    public string NameAr { get; set; } = "السياسة الافتراضية";
    public string NameEn { get; set; } = "Default Policy";

    /// <summary>Default grace (minutes) applied when a shift has no grace-after-start configured.</summary>
    public int DefaultGraceMinutes { get; set; } = 0;

    /// <summary>Round worked minutes to the nearest N minutes (0 = no rounding).</summary>
    public int RoundingMinutes { get; set; } = 0;

    /// <summary>Mark a working day with no punches as Absent (vs. leaving it blank).</summary>
    public bool AutoMarkAbsent { get; set; } = true;

    /// <summary>Count overtime only when the shift allows it.</summary>
    public bool CountOvertime { get; set; } = true;

    public bool IsDefault { get; set; } = true;
    public bool IsActive { get; set; } = true;
}
