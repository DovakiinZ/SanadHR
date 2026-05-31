namespace HR.Modules.Platform.DTOs.Workflows;

public class WorkflowVersionDetailDto
{
    public Guid Id { get; set; }
    public Guid WorkflowDefinitionId { get; set; }
    public int VersionNumber { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public List<WorkflowNodeDto> Nodes { get; set; } = new();
    public List<WorkflowEdgeDto> Edges { get; set; } = new();
}

public class WorkflowNodeDto
{
    public Guid Id { get; set; }
    public string NodeType { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? Configuration { get; set; }
    public int PositionX { get; set; }
    public int PositionY { get; set; }
    public List<WorkflowConditionDto> Conditions { get; set; } = new();
    public List<WorkflowApproverRuleDto> ApproverRules { get; set; } = new();
}

public class WorkflowEdgeDto
{
    public Guid Id { get; set; }
    public Guid SourceNodeId { get; set; }
    public Guid TargetNodeId { get; set; }
    public string? Condition { get; set; }
    public int SortOrder { get; set; }
}

public class WorkflowConditionDto
{
    public Guid Id { get; set; }
    public string Field { get; set; } = null!;
    public string Operator { get; set; } = null!;
    public string Value { get; set; } = null!;
    public string? LogicalOperator { get; set; }
}

public class WorkflowApproverRuleDto
{
    public Guid Id { get; set; }
    public string ApproverType { get; set; } = null!;
    public Guid? SpecificUserId { get; set; }
    public Guid? SpecificRoleId { get; set; }
}
