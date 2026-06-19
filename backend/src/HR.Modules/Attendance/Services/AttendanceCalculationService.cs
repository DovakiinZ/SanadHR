using HR.Domain.Engines.Attendance;
using HR.Domain.Enums;

namespace HR.Modules.Attendance.Services;

/// <summary>Outcome of computing one employee/day. All durations are in minutes.</summary>
public sealed class AttendanceCalcResult
{
    public AttendanceStatus Status { get; set; }
    public int RequiredMinutes { get; set; }
    public int WorkedMinutes { get; set; }
    public int LateMinutes { get; set; }
    public int ShortageMinutes { get; set; }
    public int OvertimeMinutes { get; set; }
    public int BreakMinutes { get; set; }
    public bool IsFlexible { get; set; }
}

/// <summary>Tenant attendance-policy values that tune the calculation (defaults when null).</summary>
public readonly record struct AttendancePolicySettings(
    int DefaultGraceMinutes, int RoundingMinutes, bool CountOvertime, bool AutoMarkAbsent);

public interface IAttendanceCalculationService
{
    bool IsWeekend(Shift? shift, DateTime date);

    /// <summary>Pure, server-side calculation for a single employee/day. Pass the resolved shift
    /// (null = default 8h, no fixed schedule), the day's first check-in / last check-out, excused-day
    /// flags, and the tenant policy. Late/shortage/overtime are derived from the shift's rules and
    /// grace windows; the policy supplies a fallback grace, worked-minute rounding, and overtime gate.</summary>
    AttendanceCalcResult Calculate(
        Shift? shift, DateTime date, DateTime? checkIn, DateTime? checkOut,
        bool isLeave = false, bool isHoliday = false, bool isWorkFromHome = false,
        AttendancePolicySettings? policy = null);
}

public sealed class AttendanceCalculationService : IAttendanceCalculationService
{
    private const int DefaultRequiredMinutes = 480; // 8h fallback when no shift is assigned

    public bool IsWeekend(Shift? shift, DateTime date)
    {
        foreach (var d in ParseWeekendDays(shift?.WeekendDays))
            if ((int)date.DayOfWeek == d) return true;
        return false;
    }

    public AttendanceCalcResult Calculate(
        Shift? shift, DateTime date, DateTime? checkIn, DateTime? checkOut,
        bool isLeave = false, bool isHoliday = false, bool isWorkFromHome = false,
        AttendancePolicySettings? policy = null)
    {
        var required = shift?.RequiredMinutes ?? DefaultRequiredMinutes;
        var breakMin = shift?.BreakMinutes ?? 0;
        var flexible = shift?.IsFlexible ?? false;

        var r = new AttendanceCalcResult
        {
            RequiredMinutes = required,
            BreakMinutes = breakMin,
            IsFlexible = flexible,
        };

        // Excused / non-working days short-circuit (no shortage penalty).
        if (isLeave) { r.Status = AttendanceStatus.OnLeave; r.RequiredMinutes = 0; return r; }
        if (isHoliday) { r.Status = AttendanceStatus.Holiday; r.RequiredMinutes = 0; return r; }
        if (IsWeekend(shift, date)) { r.Status = AttendanceStatus.Weekend; r.RequiredMinutes = 0; return r; }

        // Missing punches.
        if (checkIn is null && checkOut is null)
        {
            r.Status = AttendanceStatus.Absent;
            r.ShortageMinutes = required;
            return r;
        }
        if (checkIn is not null && checkOut is null)
        {
            r.Status = AttendanceStatus.MissingCheckOut;
            r.ShortageMinutes = required;
            return r;
        }
        if (checkIn is null && checkOut is not null)
        {
            r.Status = AttendanceStatus.MissingCheckIn;
            r.ShortageMinutes = required;
            return r;
        }

        // Both punches present → compute worked time (handle overnight shifts).
        var inT = checkIn!.Value;
        var outT = checkOut!.Value;
        var gross = (int)Math.Round((outT - inT).TotalMinutes);
        if (gross < 0) gross += 24 * 60; // overnight
        var worked = Math.Max(0, gross - breakMin);
        // Optional worked-minute rounding (policy), e.g. round to the nearest 5/15 minutes.
        if (policy is { RoundingMinutes: > 0 } pr)
            worked = (int)(Math.Round((double)worked / pr.RoundingMinutes) * pr.RoundingMinutes);
        r.WorkedMinutes = worked;

        // Lateness only applies to fixed shifts. Grace falls back to the policy default when the shift
        // doesn't set one.
        if (!flexible && shift is not null)
        {
            var graceAfterStart = shift.GraceAfterStartMinutes > 0 ? shift.GraceAfterStartMinutes : (policy?.DefaultGraceMinutes ?? 0);
            var scheduledStart = date.Date.Add(shift.StartTime.ToTimeSpan());
            var allowedStart = scheduledStart.AddMinutes(graceAfterStart);
            var lateMin = (int)Math.Round((inT - allowedStart).TotalMinutes);
            r.LateMinutes = Math.Max(0, lateMin);
        }

        r.ShortageMinutes = Math.Max(0, required - worked);

        var overtimeAllowed = (shift?.OvertimeAllowed ?? false) && (policy?.CountOvertime ?? true);
        if (overtimeAllowed)
            r.OvertimeMinutes = Math.Max(0, worked - required);

        // Resolve a single headline status (minute fields stay populated regardless).
        if (isWorkFromHome) r.Status = AttendanceStatus.WorkFromHome;
        else if (r.LateMinutes > 0) r.Status = AttendanceStatus.Late;
        else if (r.ShortageMinutes > 0) r.Status = AttendanceStatus.ShortHours;
        else if (r.OvertimeMinutes > 0) r.Status = AttendanceStatus.Overtime;
        else r.Status = AttendanceStatus.Present;

        return r;
    }

    /// <summary>Parse a "5,6" weekend CSV into DayOfWeek ints. Defaults to Fri+Sat.</summary>
    public static IEnumerable<int> ParseWeekendDays(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv)) { yield return 5; yield return 6; yield break; }
        foreach (var part in csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            if (int.TryParse(part, out var d) && d is >= 0 and <= 6) yield return d;
    }
}
