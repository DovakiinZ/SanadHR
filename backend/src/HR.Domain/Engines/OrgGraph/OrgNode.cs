using HR.Domain.Common;

namespace HR.Domain.Engines.OrgGraph;

public class OrgNode : TenantEntity
{
    public string NodeType { get; set; } = null!; // Department, Branch, Position
    public Guid EntityId { get; set; } // FK to Department/Branch/Position
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public Guid? ParentNodeId { get; set; }
    public int Level { get; set; }
    public int PositionX { get; set; }
    public int PositionY { get; set; }
    public string? Metadata { get; set; } // JSONB - extra display data
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public OrgNode? ParentNode { get; set; }
    public ICollection<OrgNode> ChildNodes { get; set; } = new List<OrgNode>();
    public ICollection<OrgEdge> OutgoingEdges { get; set; } = new List<OrgEdge>();
    public ICollection<OrgEdge> IncomingEdges { get; set; } = new List<OrgEdge>();
}
