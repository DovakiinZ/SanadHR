using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Dashboards;

public class DashboardWidget : BaseEntity
{
    public Guid DashboardDefinitionId { get; set; }
    public Guid? WidgetDefinitionId { get; set; }
    public WidgetType WidgetType { get; set; }
    public Guid? ObjectDefinitionId { get; set; }
    public string TitleEn { get; set; } = null!;
    public string TitleAr { get; set; } = null!;
    public string? Configuration { get; set; } // JSONB - widget-specific config
    public string? DataSourceConfig { get; set; } // JSONB - inline data source override
    public int SortOrder { get; set; }
    public bool IsVisible { get; set; } = true;

    public DashboardDefinition DashboardDefinition { get; set; } = null!;
    public WidgetDefinition? WidgetDefinition { get; set; }
    public ICollection<WidgetFilter> Filters { get; set; } = new List<WidgetFilter>();
    public ICollection<WidgetDrilldown> Drilldowns { get; set; } = new List<WidgetDrilldown>();
    public WidgetLayout? Layout { get; set; }
}
