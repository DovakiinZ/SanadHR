using HR.Domain.Common;

namespace HR.Domain.Engines.FlowBuilder;

/// <summary>
/// An immutable record of one state transition of a <see cref="WorkflowRequest"/>.
/// Written by the execution engine on every step the engine runs, so the full
/// history of a request can be replayed for audit / UI timelines.
/// </summary>
public class WorkflowAuditTrail : TenantEntity
{
    public Guid RequestId { get; set; }
    public WorkflowRequest Request { get; set; } = null!;

    /// <summary>The step that was executed (null for request-level events like Submitted/Cancelled).</summary>
    public Guid? StepId { get; set; }
    public string? StepName { get; set; }

    /// <summary>The step the request moved to after this transition (null = branch ended).</summary>
    public Guid? ToStepId { get; set; }

    /// <summary>What happened, e.g. Submitted, Approved, Rejected, ActionExecuted, ConditionEvaluated, Completed, Cancelled.</summary>
    public string Action { get; set; } = null!;

    /// <summary>Outcome detail, e.g. "Success", "Failure", or a human note.</summary>
    public string? Result { get; set; }

    /// <summary>The user who caused the transition (null for engine-driven steps).</summary>
    public Guid? ActorId { get; set; }

    public string? Comment { get; set; }

    public DateTime OccurredAt { get; set; }
}
