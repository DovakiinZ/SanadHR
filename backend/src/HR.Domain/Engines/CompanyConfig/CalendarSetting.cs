using HR.Domain.Common;

namespace HR.Domain.Engines.CompanyConfig;

public class CalendarSetting : TenantEntity
{
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string CalendarType { get; set; } = null!; // Gregorian, Hijri
    public string WorkWeekDays { get; set; } = null!; // JSONB - [0,1,2,3,4] (Sun-Thu)
    public string? Holidays { get; set; } // JSONB
    public TimeSpan WorkDayStart { get; set; }
    public TimeSpan WorkDayEnd { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}
