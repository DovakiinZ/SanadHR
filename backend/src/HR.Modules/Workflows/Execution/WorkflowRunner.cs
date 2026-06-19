using System.Text.Json;
using HR.Domain.Engines.FlowBuilder;
using HR.Infrastructure.Persistence;

namespace HR.Modules.Workflows.Execution;

/// <summary>
/// Drives a <see cref="WorkflowRequest"/> through its step graph. Implemented as an explicit state
/// machine: it applies an optional decision to the current (blocking) step, then auto-advances
/// through non-blocking steps (Action/Condition) until it either parks on a blocking step or reaches
/// a terminal. It mutates the tracked request and appends <see cref="WorkflowAuditTrail"/> entries
/// but never calls SaveChanges — the caller commits everything in a single atomic transaction.
/// </summary>
public interface IWorkflowRunner
{
    Task AdvanceAsync(WorkflowRequest request, WorkflowDecision? decision, CancellationToken ct);
}

public class WorkflowRunner : IWorkflowRunner
{
    private readonly IReadOnlyDictionary<WorkflowStepType, IWorkflowStepHandler> _handlers;
    private readonly ApplicationDbContext _context;

    public WorkflowRunner(IEnumerable<IWorkflowStepHandler> handlers, ApplicationDbContext context)
    {
        _handlers = handlers.ToDictionary(h => h.StepType);
        _context = context;
    }

    public async Task AdvanceAsync(WorkflowRequest request, WorkflowDecision? decision, CancellationToken ct)
    {
        // Idempotent: a finished request never moves again.
        if (request.Status is WorkflowRequestStatus.Completed
            or WorkflowRequestStatus.Cancelled
            or WorkflowRequestStatus.Rejected)
            return;

        var steps = request.Definition.Steps.ToDictionary(s => s.Id);
        var payload = ParsePayload(request.Payload);

        // Empty graph => nothing to do; the request is immediately complete.
        if (request.CurrentStepId is null)
        {
            if (request.Definition.RootStepId is null)
            {
                Complete(request, WorkflowRequestStatus.Completed);
                return;
            }
            request.CurrentStepId = request.Definition.RootStepId;
        }

        request.Status = WorkflowRequestStatus.InProgress;

        // Only the first step the engine touches this call consumes the external decision; every
        // step it then auto-advances into runs unattended.
        var pendingDecision = decision;
        var guard = steps.Count + 1; // belt-and-braces against a malformed (cyclic) graph slipping through

        while (request.CurrentStepId is { } currentId && guard-- > 0)
        {
            if (!steps.TryGetValue(currentId, out var step))
            {
                // Pointer into a missing step — treat as a terminal to avoid a stuck request.
                Complete(request, WorkflowRequestStatus.Completed);
                return;
            }

            if (step.Type == WorkflowStepType.End)
            {
                AddAudit(request, step, toStepId: null, action: "Completed", result: "Reached end step", decision: null);
                Complete(request, WorkflowRequestStatus.Completed);
                return;
            }

            if (!_handlers.TryGetValue(step.Type, out var handler))
                throw new InvalidOperationException($"No handler registered for step type '{step.Type}'.");

            var result = await handler.ExecuteAsync(
                new StepExecutionContext { Request = request, Step = step, Decision = pendingDecision, Payload = payload }, ct);

            if (result.Halt)
            {
                // Blocking step with no input yet — park here and wait.
                request.Status = WorkflowRequestStatus.InProgress;
                return;
            }

            var nextId = result.Branch == WorkflowBranch.Success ? step.NextStepIdSuccess : step.NextStepIdFailure;
            AddAudit(request, step, nextId, result.Action, result.Result, pendingDecision);
            pendingDecision = null; // consumed

            if (nextId is null)
            {
                // Branch ends. A rejection with no failure branch terminates as Rejected; otherwise Completed.
                var status = result.Action == "Rejected"
                    ? WorkflowRequestStatus.Rejected
                    : WorkflowRequestStatus.Completed;
                Complete(request, status);
                return;
            }

            request.CurrentStepId = nextId;
        }
    }

    private static void Complete(WorkflowRequest request, WorkflowRequestStatus status)
    {
        request.Status = status;
        request.CurrentStepId = null;
        request.CompletedAt = DateTime.UtcNow;
    }

    private void AddAudit(WorkflowRequest request, WorkflowStep step, Guid? toStepId,
        string action, string? result, WorkflowDecision? decision)
    {
        var audit = new WorkflowAuditTrail
        {
            TenantId = request.TenantId,
            RequestId = request.Id,
            StepId = step.Id,
            StepName = step.Name,
            ToStepId = toStepId,
            Action = action,
            Result = result,
            ActorId = decision?.ActorId,
            Comment = decision?.Comment,
            OccurredAt = DateTime.UtcNow
        };
        // Add through the DbSet so EF marks it Added. Appending only to the navigation collection of
        // an already-tracked request would let EF mis-infer the client-set Guid key as an existing
        // (Modified) row. Also mirror it onto the in-memory collection for the returned DTO.
        _context.FlowAuditTrail.Add(audit);
        request.AuditTrail.Add(audit);
    }

    private static JsonElement ParsePayload(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload)) return default;
        try
        {
            using var doc = JsonDocument.Parse(payload);
            return doc.RootElement.Clone();
        }
        catch (JsonException)
        {
            return default;
        }
    }
}
