namespace HR.Modules.Attendance.DTOs;

public sealed class AttendanceHolidayDto
{
    public Guid Id { get; set; }
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string Date { get; set; } = null!;   // yyyy-MM-dd
    public bool IsRecurring { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class HolidayInput
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public DateTime Date { get; set; }
    public bool IsRecurring { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class AttendancePolicyDto
{
    public Guid Id { get; set; }
    public string NameAr { get; set; } = "السياسة الافتراضية";
    public string NameEn { get; set; } = "Default Policy";
    public int DefaultGraceMinutes { get; set; }
    public int RoundingMinutes { get; set; }
    public bool AutoMarkAbsent { get; set; } = true;
    public bool CountOvertime { get; set; } = true;
    public bool IsActive { get; set; } = true;
}

public sealed class PolicyInput
{
    public int DefaultGraceMinutes { get; set; }
    public int RoundingMinutes { get; set; }
    public bool AutoMarkAbsent { get; set; } = true;
    public bool CountOvertime { get; set; } = true;
}
