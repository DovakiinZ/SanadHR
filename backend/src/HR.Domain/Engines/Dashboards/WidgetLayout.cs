using HR.Domain.Common;

namespace HR.Domain.Engines.Dashboards;

public class WidgetLayout : BaseEntity
{
    public Guid DashboardWidgetId { get; set; }
    public int Column { get; set; }
    public int Row { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public DashboardWidget DashboardWidget { get; set; } = null!;
}
