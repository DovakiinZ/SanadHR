using HR.Domain.Common;

namespace HR.Domain.Engines.Workflows;

public class WorkflowEdge : BaseEntity
{
    public Guid WorkflowVersionId { get; set; }
    public Guid SourceNodeId { get; set; }
    public Guid TargetNodeId { get; set; }
    public string? Condition { get; set; } // JSONB
    public int SortOrder { get; set; }

    public WorkflowVersion WorkflowVersion { get; set; } = null!;
    public WorkflowNode SourceNode { get; set; } = null!;
    public WorkflowNode TargetNode { get; set; } = null!;
}
