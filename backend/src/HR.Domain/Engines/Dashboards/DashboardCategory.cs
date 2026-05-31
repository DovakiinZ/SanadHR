using HR.Domain.Common;

namespace HR.Domain.Engines.Dashboards;

public class DashboardCategory : TenantEntity
{
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<DashboardDefinition> Dashboards { get; set; } = new List<DashboardDefinition>();
}
