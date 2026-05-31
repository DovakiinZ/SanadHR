using HR.Domain.Common;

namespace HR.Domain.Engines.Dashboards;

public class DashboardShare : BaseEntity
{
    public Guid DashboardDefinitionId { get; set; }
    public Guid? SharedWithUserId { get; set; }
    public Guid? SharedWithDepartmentId { get; set; }
    public Guid? SharedWithRoleId { get; set; }
    public bool CanEdit { get; set; }
    public DateTime SharedAt { get; set; }
    public string? SharedBy { get; set; }

    public DashboardDefinition DashboardDefinition { get; set; } = null!;
}
