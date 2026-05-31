using HR.Domain.Common;

namespace HR.Domain.Engines.Dashboards;

public class WidgetFilter : BaseEntity
{
    public Guid DashboardWidgetId { get; set; }
    public string FieldCode { get; set; } = null!;
    public string Operator { get; set; } = null!;
    public string Value { get; set; } = null!;

    public DashboardWidget DashboardWidget { get; set; } = null!;
}
