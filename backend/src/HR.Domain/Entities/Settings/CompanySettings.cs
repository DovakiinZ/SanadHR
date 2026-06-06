using HR.Domain.Common;

namespace HR.Modules.Settings.Entities;

public class CompanySettings : TenantEntity
{
    public string CompanyName { get; set; } = null!;
    public string? CompanyNameAr { get; set; }
    public string? LogoUrl { get; set; }
    public string? Website { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; } = "SA";
    public string? Currency { get; set; } = "SAR";
    public string? Timezone { get; set; } = "Asia/Riyadh";
    public string? DateFormat { get; set; } = "dd/MM/yyyy";
    public int WorkingDaysPerWeek { get; set; } = 5;
    public string? WeekStartDay { get; set; } = "Sunday";
    public int AnnualLeaveDays { get; set; } = 21;
}
