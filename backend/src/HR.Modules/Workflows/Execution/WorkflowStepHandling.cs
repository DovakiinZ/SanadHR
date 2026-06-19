using System.Text.Json;
using HR.Domain.Engines.FlowBuilder;

namespace HR.Modules.Workflows.Execution;

/// <summary>Which outgoing pointer the engine should follow after a step runs.</summary>
public enum WorkflowBranch
{
    Success,
    Failure
}

/// <summary>
/// A decision supplied from the outside (e.g. an approver's verdict) that a blocking step consumes.
/// Null when the engine is auto-advancing through non-blocking steps.
/// </summary>
public record WorkflowDecision(bool Approved, string? Comment, Guid? ActorId);

/// <summary>Everything a step handler needs to execute one step. Read-only — handlers must not mutate persistence.</summary>
public class StepExecutionContext
{
    public required WorkflowRequest Request { get; init; }
    public required WorkflowStep Step { get; init; }

    /// <summary>The external decision for this run, if any. Only meaningful for blocking steps.</summary>
    public WorkflowDecision? Decision { get; init; }

    /// <summary>The request payload parsed once, shared across handlers.</summary>
    public required JsonElement Payload { get; init; }
}

/// <summary>
/// The outcome of executing a single step.
/// <para><see cref="Halt"/> = the step is blocking and cannot complete yet (e.g. an approval is
/// waiting for a human). The engine parks the request on this step and stops.</para>
/// </summary>
public record StepExecutionResult(WorkflowBranch Branch, bool Halt, string Action, string? Result)
{
    public static StepExecutionResult Wait(string action = "Pending")
        => new(WorkflowBranch.Success, true, action, "Awaiting input");

    public static StepExecutionResult Continue(WorkflowBranch branch, string action, string? result = null)
        => new(branch, false, action, result);
}

/// <summary>
/// Strategy for executing one kind of <see cref="WorkflowStep"/>. New step types are added by
/// implementing this interface and registering it in DI — the engine never changes (Open/Closed).
/// </summary>
public interface IWorkflowStepHandler
{
    WorkflowStepType StepType { get; }
    Task<StepExecutionResult> ExecuteAsync(StepExecutionContext context, CancellationToken ct);
}
