using HR.Domain.Common;

namespace HR.Domain.Engines.Attendance;

/// <summary>An official (company/public) holiday. On a holiday date every employee's day resolves to
/// status Holiday with no shortage penalty, regardless of shift. Recurring holidays repeat every year
/// on the same month/day (e.g. National Day).</summary>
public class AttendanceHoliday : TenantEntity
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;

    /// <summary>The holiday date. For recurring holidays only month/day are significant.</summary>
    public DateTime Date { get; set; }

    public bool IsRecurring { get; set; }
    public bool IsActive { get; set; } = true;
}
