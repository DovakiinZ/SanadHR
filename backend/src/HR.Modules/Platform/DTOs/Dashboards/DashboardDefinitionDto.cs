namespace HR.Modules.Platform.DTOs.Dashboards;

public class DashboardDefinitionDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryNameEn { get; set; }
    public string Scope { get; set; } = null!;
    public Guid? OwnerId { get; set; }
    public Guid? DepartmentId { get; set; }
    public bool IsDefault { get; set; }
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; }
    public string? LayoutConfiguration { get; set; }
    public int SortOrder { get; set; }
    public List<DashboardWidgetDto> Widgets { get; set; } = new();
    public List<DashboardShareDto> Shares { get; set; } = new();
}

public class DashboardCategoryDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public int DashboardCount { get; set; }
}

public class DashboardTemplateDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? Description { get; set; }
    public string? PreviewImageUrl { get; set; }
    public string DefaultScope { get; set; } = null!;
    public string LayoutConfiguration { get; set; } = null!;
    public string WidgetConfiguration { get; set; } = null!;
    public bool IsSystem { get; set; }
    public int SortOrder { get; set; }
}

public class DashboardShareDto
{
    public Guid Id { get; set; }
    public Guid? SharedWithUserId { get; set; }
    public Guid? SharedWithDepartmentId { get; set; }
    public Guid? SharedWithRoleId { get; set; }
    public bool CanEdit { get; set; }
    public DateTime SharedAt { get; set; }
}

public class DashboardWidgetDto
{
    public Guid Id { get; set; }
    public Guid? WidgetDefinitionId { get; set; }
    public string WidgetType { get; set; } = null!;
    public Guid? ObjectDefinitionId { get; set; }
    public string TitleEn { get; set; } = null!;
    public string TitleAr { get; set; } = null!;
    public string? Configuration { get; set; }
    public string? DataSourceConfig { get; set; }
    public int SortOrder { get; set; }
    public bool IsVisible { get; set; }
    public WidgetLayoutDto? Layout { get; set; }
    public List<WidgetFilterDto> Filters { get; set; } = new();
    public List<WidgetDrilldownDto> Drilldowns { get; set; } = new();
}

public class WidgetDefinitionDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? Description { get; set; }
    public string WidgetType { get; set; } = null!;
    public string? Icon { get; set; }
    public string? DefaultConfiguration { get; set; }
    public int DefaultWidth { get; set; }
    public int DefaultHeight { get; set; }
    public bool IsSystem { get; set; }
    public int SortOrder { get; set; }
    public List<WidgetDataSourceDto> DataSources { get; set; } = new();
}

public class WidgetDataSourceDto
{
    public Guid Id { get; set; }
    public string SourceType { get; set; } = null!;
    public Guid? ObjectDefinitionId { get; set; }
    public string? QueryTemplate { get; set; }
    public string? ApiEndpoint { get; set; }
    public string? Aggregation { get; set; }
    public string? AggregationField { get; set; }
    public string? GroupByField { get; set; }
    public string? DateRangeField { get; set; }
    public int RefreshIntervalSeconds { get; set; }
}

public class WidgetDrilldownDto
{
    public Guid Id { get; set; }
    public string DrilldownType { get; set; } = null!;
    public Guid? TargetDashboardId { get; set; }
    public string? TargetRoute { get; set; }
    public string? FilterMapping { get; set; }
    public string? LabelEn { get; set; }
    public string? LabelAr { get; set; }
}

public class WidgetLayoutDto
{
    public Guid Id { get; set; }
    public int Column { get; set; }
    public int Row { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

public class WidgetFilterDto
{
    public Guid Id { get; set; }
    public string FieldCode { get; set; } = null!;
    public string Operator { get; set; } = null!;
    public string Value { get; set; } = null!;
}
