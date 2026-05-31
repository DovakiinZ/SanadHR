namespace HR.Modules.Platform.DTOs.Dashboards;

public class DashboardDefinitionDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public bool IsDefault { get; set; }
    public bool IsSystem { get; set; }
    public List<DashboardWidgetDto> Widgets { get; set; } = new();
}

public class DashboardWidgetDto
{
    public Guid Id { get; set; }
    public string WidgetType { get; set; } = null!;
    public Guid? ObjectDefinitionId { get; set; }
    public string Title { get; set; } = null!;
    public string? Configuration { get; set; }
    public int SortOrder { get; set; }
    public WidgetLayoutDto? Layout { get; set; }
    public List<WidgetFilterDto> Filters { get; set; } = new();
}

public class WidgetLayoutDto
{
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
