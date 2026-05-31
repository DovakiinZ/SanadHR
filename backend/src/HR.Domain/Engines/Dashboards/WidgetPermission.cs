using HR.Domain.Common;

namespace HR.Domain.Engines.Dashboards;

public class WidgetPermission : BaseEntity
{
    public Guid WidgetDefinitionId { get; set; }
    public string PermissionCode { get; set; } = null!;

    public WidgetDefinition WidgetDefinition { get; set; } = null!;
}
