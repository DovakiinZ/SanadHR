using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Workflows;

public class WorkflowInstance : TenantEntity
{
    public Guid WorkflowDefinitionId { get; set; }
    public Guid WorkflowVersionId { get; set; }
    public string EntityType { get; set; } = null!;
    public Guid EntityId { get; set; }
    public WorkflowStatus Status { get; set; } = WorkflowStatus.Active;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public WorkflowDefinition WorkflowDefinition { get; set; } = null!;
    public WorkflowVersion WorkflowVersion { get; set; } = null!;
    public ICollection<WorkflowInstanceStep> Steps { get; set; } = new List<WorkflowInstanceStep>();
}
