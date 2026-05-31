using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Dashboards;

public class WidgetDataSource : BaseEntity
{
    public Guid WidgetDefinitionId { get; set; }
    public DataSourceType SourceType { get; set; }
    public Guid? ObjectDefinitionId { get; set; }
    public string? QueryTemplate { get; set; } // JSONB - query config
    public string? ApiEndpoint { get; set; }
    public AggregationType? Aggregation { get; set; }
    public string? AggregationField { get; set; }
    public string? GroupByField { get; set; }
    public string? DateRangeField { get; set; }
    public int RefreshIntervalSeconds { get; set; } = 300;
    public int SortOrder { get; set; }

    public WidgetDefinition WidgetDefinition { get; set; } = null!;
}
