using HR.Domain.Common;

namespace HR.Modules.Dashboards.Entities;

// TODO: Implement dashboard widget entity
public class DashboardWidget : TenantEntity
{
    public string Name { get; set; } = null!;
    public string WidgetType { get; set; } = null!;
    public string? Configuration { get; set; }
    public int SortOrder { get; set; }
}
