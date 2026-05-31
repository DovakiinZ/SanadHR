using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Dashboards;

public class DashboardTemplate : TenantEntity
{
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? Description { get; set; }
    public string? PreviewImageUrl { get; set; }
    public DashboardScope DefaultScope { get; set; }
    public string LayoutConfiguration { get; set; } = null!; // JSONB - default grid
    public string WidgetConfiguration { get; set; } = null!; // JSONB - default widgets
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
