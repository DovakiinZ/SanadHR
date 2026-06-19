using HR.Domain.Common;

namespace HR.Domain.Engines.FlowBuilder;

/// <summary>
/// A running instance of a <see cref="WorkflowDefinition"/> — the unit the execution engine
/// advances through the step graph. State is captured by <see cref="Status"/> and
/// <see cref="CurrentStepId"/>; every transition is recorded in <see cref="AuditTrail"/>.
/// </summary>
public class WorkflowRequest : TenantEntity
{
    /// <summary>Human reference, e.g. "WF-2026-000123".</summary>
    public string RequestNumber { get; set; } = null!;

    public Guid DefinitionId { get; set; }
    public WorkflowDefinition Definition { get; set; } = null!;

    /// <summary>The user this request belongs to / was raised by.</summary>
    public Guid RequesterId { get; set; }

    /// <summary>The step the engine is currently parked on. Null once completed/cancelled.</summary>
    public Guid? CurrentStepId { get; set; }

    public WorkflowRequestStatus Status { get; set; } = WorkflowRequestStatus.Pending;

    /// <summary>The data being processed (e.g. leave dates), stored as JSON (jsonb).</summary>
    public string Payload { get; set; } = "{}";

    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public ICollection<WorkflowAuditTrail> AuditTrail { get; set; } = new List<WorkflowAuditTrail>();
}
