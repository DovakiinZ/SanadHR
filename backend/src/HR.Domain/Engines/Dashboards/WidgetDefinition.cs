using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Dashboards;

public class WidgetDefinition : TenantEntity
{
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? Description { get; set; }
    public WidgetType WidgetType { get; set; }
    public string? Icon { get; set; }
    public string? DefaultConfiguration { get; set; } // JSONB
    public int DefaultWidth { get; set; } = 4;
    public int DefaultHeight { get; set; } = 3;
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    public ICollection<WidgetDataSource> DataSources { get; set; } = new List<WidgetDataSource>();
    public ICollection<WidgetPermission> Permissions { get; set; } = new List<WidgetPermission>();
}
