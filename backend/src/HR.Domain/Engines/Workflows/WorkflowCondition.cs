using HR.Domain.Common;

namespace HR.Domain.Engines.Workflows;

public class WorkflowCondition : BaseEntity
{
    public Guid WorkflowNodeId { get; set; }
    public string Field { get; set; } = null!;
    public string Operator { get; set; } = null!;
    public string Value { get; set; } = null!;
    public string? LogicalOperator { get; set; } // AND / OR

    public WorkflowNode WorkflowNode { get; set; } = null!;
}
