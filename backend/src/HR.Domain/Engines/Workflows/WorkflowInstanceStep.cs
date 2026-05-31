using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Workflows;

public class WorkflowInstanceStep : BaseEntity
{
    public Guid WorkflowInstanceId { get; set; }
    public Guid WorkflowNodeId { get; set; }
    public Guid? AssignedToId { get; set; }
    public WorkflowStepStatus Status { get; set; } = WorkflowStepStatus.Pending;
    public DateTime? ActionTakenAt { get; set; }
    public string? Comment { get; set; }
    public WorkflowActionType? ActionType { get; set; }

    public WorkflowInstance WorkflowInstance { get; set; } = null!;
    public WorkflowNode WorkflowNode { get; set; } = null!;
}
