using HR.Domain.Engines.FlowBuilder;

namespace HR.Modules.Workflows.DTOs;

/// <summary>A step as exchanged with the builder UI.</summary>
public record WorkflowStepDto
{
    public Guid Id { get; init; }
    public WorkflowStepType Type { get; init; }
    public string Name { get; init; } = null!;
    public string Config { get; init; } = "{}";
    public Guid? NextStepIdSuccess { get; init; }
    public Guid? NextStepIdFailure { get; init; }
    public int SortOrder { get; init; }
}

public record WorkflowDefinitionDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public int Version { get; init; }
    public bool IsActive { get; init; }
    public Guid? RootStepId { get; init; }
    public List<WorkflowStepDto> Steps { get; init; } = new();
}

/// <summary>List-row projection (no step graph).</summary>
public record WorkflowDefinitionSummaryDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public int Version { get; init; }
    public bool IsActive { get; init; }
    public int StepCount { get; init; }
    public int RequestCount { get; init; }
}

public record WorkflowAuditTrailDto
{
    public Guid Id { get; init; }
    public Guid? StepId { get; init; }
    public string? StepName { get; init; }
    public Guid? ToStepId { get; init; }
    public string Action { get; init; } = null!;
    public string? Result { get; init; }
    public Guid? ActorId { get; init; }
    public string? Comment { get; init; }
    public DateTime OccurredAt { get; init; }
}

public record WorkflowRequestDto
{
    public Guid Id { get; init; }
    public string RequestNumber { get; init; } = null!;
    public Guid DefinitionId { get; init; }
    public string DefinitionName { get; init; } = null!;
    public Guid RequesterId { get; init; }
    public Guid? CurrentStepId { get; init; }
    public string? CurrentStepName { get; init; }
    public WorkflowRequestStatus Status { get; init; }
    public string Payload { get; init; } = "{}";
    public DateTime StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public List<WorkflowAuditTrailDto> AuditTrail { get; init; } = new();
}

/// <summary>Input shape for one step when saving a definition's graph.</summary>
public record WorkflowStepInput
{
    /// <summary>Client-generated id; reused as the persisted id so success/failure pointers stay valid.</summary>
    public Guid Id { get; init; }
    public WorkflowStepType Type { get; init; }
    public string Name { get; init; } = null!;
    public string Config { get; init; } = "{}";
    public Guid? NextStepIdSuccess { get; init; }
    public Guid? NextStepIdFailure { get; init; }
    public int SortOrder { get; init; }
}

/// <summary>Mapping helpers (kept manual, matching the codebase convention).</summary>
public static class WorkflowMappingExtensions
{
    public static WorkflowStepDto ToDto(this WorkflowStep s) => new()
    {
        Id = s.Id,
        Type = s.Type,
        Name = s.Name,
        Config = s.Config,
        NextStepIdSuccess = s.NextStepIdSuccess,
        NextStepIdFailure = s.NextStepIdFailure,
        SortOrder = s.SortOrder
    };

    public static WorkflowDefinitionDto ToDto(this WorkflowDefinition d) => new()
    {
        Id = d.Id,
        Code = d.Code,
        Name = d.Name,
        Description = d.Description,
        Version = d.Version,
        IsActive = d.IsActive,
        RootStepId = d.RootStepId,
        Steps = d.Steps.OrderBy(s => s.SortOrder).Select(s => s.ToDto()).ToList()
    };

    public static WorkflowAuditTrailDto ToDto(this WorkflowAuditTrail a) => new()
    {
        Id = a.Id,
        StepId = a.StepId,
        StepName = a.StepName,
        ToStepId = a.ToStepId,
        Action = a.Action,
        Result = a.Result,
        ActorId = a.ActorId,
        Comment = a.Comment,
        OccurredAt = a.OccurredAt
    };

    public static WorkflowRequestDto ToDto(this WorkflowRequest r) => new()
    {
        Id = r.Id,
        RequestNumber = r.RequestNumber,
        DefinitionId = r.DefinitionId,
        DefinitionName = r.Definition?.Name ?? string.Empty,
        RequesterId = r.RequesterId,
        CurrentStepId = r.CurrentStepId,
        CurrentStepName = r.CurrentStepId != null
            ? r.Definition?.Steps.FirstOrDefault(s => s.Id == r.CurrentStepId)?.Name
            : null,
        Status = r.Status,
        Payload = r.Payload,
        StartedAt = r.StartedAt,
        CompletedAt = r.CompletedAt,
        AuditTrail = r.AuditTrail.OrderBy(a => a.OccurredAt).Select(a => a.ToDto()).ToList()
    };
}
