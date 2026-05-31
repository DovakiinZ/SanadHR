using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Workflows;

public class WorkflowNode : BaseEntity
{
    public Guid WorkflowVersionId { get; set; }
    public WorkflowNodeType NodeType { get; set; }
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? Configuration { get; set; } // JSONB
    public int PositionX { get; set; }
    public int PositionY { get; set; }

    public WorkflowVersion WorkflowVersion { get; set; } = null!;
    public ICollection<WorkflowCondition> Conditions { get; set; } = new List<WorkflowCondition>();
    public ICollection<WorkflowApproverRule> ApproverRules { get; set; } = new List<WorkflowApproverRule>();
    public ICollection<WorkflowEdge> OutgoingEdges { get; set; } = new List<WorkflowEdge>();
    public ICollection<WorkflowEdge> IncomingEdges { get; set; } = new List<WorkflowEdge>();
    public ICollection<WorkflowInstanceStep> InstanceSteps { get; set; } = new List<WorkflowInstanceStep>();
}
