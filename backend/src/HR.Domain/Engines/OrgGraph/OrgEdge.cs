using HR.Domain.Common;

namespace HR.Domain.Engines.OrgGraph;

public class OrgEdge : BaseEntity
{
    public Guid SourceNodeId { get; set; }
    public Guid TargetNodeId { get; set; }
    public string RelationType { get; set; } = null!; // Reports-To, Manages, Belongs-To
    public string? Label { get; set; }
    public bool IsActive { get; set; } = true;

    public OrgNode SourceNode { get; set; } = null!;
    public OrgNode TargetNode { get; set; } = null!;
}
