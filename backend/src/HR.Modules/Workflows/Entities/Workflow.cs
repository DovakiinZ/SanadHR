using HR.Domain.Common;

namespace HR.Modules.Workflows.Entities;

// TODO: Implement workflow entity
public class Workflow : TenantEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
