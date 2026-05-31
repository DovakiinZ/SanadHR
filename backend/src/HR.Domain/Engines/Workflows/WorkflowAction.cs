using HR.Domain.Common;

namespace HR.Domain.Engines.Workflows;

public class WorkflowAction : BaseEntity
{
    public Guid WorkflowNodeId { get; set; }
    public string ActionType { get; set; } = null!; // Approve, Reject, Return, Notify, CreateTask, GenerateDocument, UpdateEmployeeData, UpdateLeaveBalance, UpdatePayroll, TriggerAutomation
    public string? Configuration { get; set; } // JSONB - action-specific config
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public WorkflowNode WorkflowNode { get; set; } = null!;
}
