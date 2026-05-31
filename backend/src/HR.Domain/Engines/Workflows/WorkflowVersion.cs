using HR.Domain.Common;

namespace HR.Domain.Engines.Workflows;

public class WorkflowVersion : BaseEntity
{
    public Guid WorkflowDefinitionId { get; set; }
    public int VersionNumber { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? Configuration { get; set; } // JSONB

    public WorkflowDefinition WorkflowDefinition { get; set; } = null!;
    public ICollection<WorkflowNode> Nodes { get; set; } = new List<WorkflowNode>();
    public ICollection<WorkflowEdge> Edges { get; set; } = new List<WorkflowEdge>();
}
