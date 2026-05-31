namespace HR.Modules.Platform.DTOs.Workflows;

public class WorkflowDefinitionDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string TriggerEntityType { get; set; } = null!;
    public bool IsActive { get; set; }
    public List<WorkflowVersionDto> Versions { get; set; } = new();
}

public class WorkflowVersionDto
{
    public Guid Id { get; set; }
    public int VersionNumber { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
}

public class WorkflowInstanceDto
{
    public Guid Id { get; set; }
    public Guid WorkflowDefinitionId { get; set; }
    public string EntityType { get; set; } = null!;
    public Guid EntityId { get; set; }
    public string Status { get; set; } = null!;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<WorkflowInstanceStepDto> Steps { get; set; } = new();
}

public class WorkflowInstanceStepDto
{
    public Guid Id { get; set; }
    public Guid WorkflowNodeId { get; set; }
    public Guid? AssignedToId { get; set; }
    public string Status { get; set; } = null!;
    public DateTime? ActionTakenAt { get; set; }
    public string? Comment { get; set; }
    public string? ActionType { get; set; }
}
