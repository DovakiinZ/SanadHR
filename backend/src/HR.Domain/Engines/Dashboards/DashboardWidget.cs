using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Dashboards;

public class DashboardWidget : BaseEntity
{
    public Guid DashboardDefinitionId { get; set; }
    public WidgetType WidgetType { get; set; }
    public Guid? ObjectDefinitionId { get; set; }
    public string Title { get; set; } = null!;
    public string? Configuration { get; set; } // JSONB
    public int SortOrder { get; set; }

    public DashboardDefinition DashboardDefinition { get; set; } = null!;
    public ICollection<WidgetFilter> Filters { get; set; } = new List<WidgetFilter>();
    public WidgetLayout? Layout { get; set; }
}
