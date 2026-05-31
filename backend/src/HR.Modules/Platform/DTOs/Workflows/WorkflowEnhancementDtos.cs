namespace HR.Modules.Platform.DTOs.Workflows;

public record WorkflowDynamicApproverDto
{
    public Guid Id { get; init; }
    public Guid WorkflowNodeId { get; init; }
    public string ApproverStrategy { get; init; } = null!;
    public Guid? SpecificEntityId { get; init; }
    public int ChainLevel { get; init; }
    public string? FallbackStrategy { get; init; }
    public Guid? FallbackEntityId { get; init; }
    public int SortOrder { get; init; }
}

public record WorkflowDynamicConditionDto
{
    public Guid Id { get; init; }
    public Guid WorkflowNodeId { get; init; }
    public string ConditionType { get; init; } = null!;
    public string FieldPath { get; init; } = null!;
    public string Operator { get; init; } = null!;
    public string Value { get; init; } = null!;
    public string? LogicalOperator { get; init; }
    public int SortOrder { get; init; }
}

public record WorkflowActionDto
{
    public Guid Id { get; init; }
    public Guid WorkflowNodeId { get; init; }
    public string ActionType { get; init; } = null!;
    public string? Configuration { get; init; }
    public int SortOrder { get; init; }
    public bool IsActive { get; init; }
}

public record WorkflowSimulationDto
{
    public Guid Id { get; init; }
    public Guid WorkflowVersionId { get; init; }
    public string InputData { get; init; } = null!;
    public string Result { get; init; } = null!;
    public DateTime SimulatedAt { get; init; }
    public Guid SimulatedById { get; init; }
}
