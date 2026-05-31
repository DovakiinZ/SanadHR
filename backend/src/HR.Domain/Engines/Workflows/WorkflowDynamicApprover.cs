using HR.Domain.Common;

namespace HR.Domain.Engines.Workflows;

public class WorkflowDynamicApprover : BaseEntity
{
    public Guid WorkflowNodeId { get; set; }
    public string ApproverStrategy { get; set; } = null!; // DirectManager, DepartmentManager, BranchManager, HRManager, PositionManager, ManagerChain, SpecificEmployee, SpecificRole, SpecificDepartment, SpecificBranch
    public Guid? SpecificEntityId { get; set; }
    public int ChainLevel { get; set; } = 1; // For manager chain
    public string? FallbackStrategy { get; set; }
    public Guid? FallbackEntityId { get; set; }
    public int SortOrder { get; set; }

    public WorkflowNode WorkflowNode { get; set; } = null!;
}
