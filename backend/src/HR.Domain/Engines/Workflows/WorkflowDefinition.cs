using HR.Domain.Common;

namespace HR.Domain.Engines.Workflows;

public class WorkflowDefinition : TenantEntity
{
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string TriggerEntityType { get; set; } = null!;
    public bool IsActive { get; set; } = true;

    public ICollection<WorkflowVersion> Versions { get; set; } = new List<WorkflowVersion>();
    public ICollection<WorkflowInstance> Instances { get; set; } = new List<WorkflowInstance>();
}
