using HR.Domain.Common;

namespace HR.Domain.Engines.Dashboards;

public class DashboardDefinition : TenantEntity
{
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public bool IsDefault { get; set; }
    public bool IsSystem { get; set; }

    public ICollection<DashboardWidget> Widgets { get; set; } = new List<DashboardWidget>();
}
