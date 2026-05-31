namespace HR.Modules.Platform.DTOs.OrgGraph;

public record OrgNodeDto
{
    public Guid Id { get; init; }
    public string NodeType { get; init; } = null!;
    public Guid EntityId { get; init; }
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public Guid? ParentNodeId { get; init; }
    public int Level { get; init; }
    public int PositionX { get; init; }
    public int PositionY { get; init; }
    public string? Metadata { get; init; }
    public int SortOrder { get; init; }
    public bool IsActive { get; init; }
    public List<OrgNodeDto> ChildNodes { get; init; } = new();
}

public record OrgEdgeDto
{
    public Guid Id { get; init; }
    public Guid SourceNodeId { get; init; }
    public Guid TargetNodeId { get; init; }
    public string RelationType { get; init; } = null!;
    public string? Label { get; init; }
    public bool IsActive { get; init; }
}

public record OrgGraphLayoutDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = null!;
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string GraphType { get; init; } = null!;
    public string LayoutData { get; init; } = null!;
    public bool IsDefault { get; init; }
    public Guid? OwnerId { get; init; }
}

public record EmployeeReportingLineDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public Guid ManagerId { get; init; }
    public string ReportingType { get; init; } = null!;
    public bool IsPrimary { get; init; }
    public DateTime EffectiveFrom { get; init; }
    public DateTime? EffectiveTo { get; init; }
    public bool IsActive { get; init; }
}

public record OrgGraphTreeDto
{
    public List<OrgNodeDto> Nodes { get; init; } = new();
    public List<OrgEdgeDto> Edges { get; init; } = new();
}
