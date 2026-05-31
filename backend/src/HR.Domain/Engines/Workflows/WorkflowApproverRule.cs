using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Workflows;

public class WorkflowApproverRule : BaseEntity
{
    public Guid WorkflowNodeId { get; set; }
    public ApproverType ApproverType { get; set; }
    public Guid? SpecificUserId { get; set; }
    public Guid? SpecificRoleId { get; set; }

    public WorkflowNode WorkflowNode { get; set; } = null!;
}
