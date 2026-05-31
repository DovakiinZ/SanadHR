using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Dashboards;

public class WidgetDrilldown : BaseEntity
{
    public Guid DashboardWidgetId { get; set; }
    public DrilldownType DrilldownType { get; set; }
    public Guid? TargetDashboardId { get; set; }
    public string? TargetRoute { get; set; }
    public string? FilterMapping { get; set; } // JSONB - maps click context to filters
    public string? LabelEn { get; set; }
    public string? LabelAr { get; set; }

    public DashboardWidget DashboardWidget { get; set; } = null!;
}
