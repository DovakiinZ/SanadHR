using HR.Domain.Engines.Requests;

namespace HR.Modules.Platform.Services.Requests;

/// <summary>
/// Drives the full request lifecycle: submit → workflow → approvals → impacts.
/// Object-driven and deterministic (impacts come from RequestImpactMapping, approval
/// chain from the linked workflow). No per-request hardcoding.
/// </summary>
public interface IRequestEngine
{
    /// <summary>Submit a request: persists the form submission, creates the instance,
    /// resolves the approval chain, starts the workflow, logs timeline/audit, notifies.</summary>
    Task<RequestInstance> SubmitAsync(Guid requestTypeId, IReadOnlyList<RequestValueInput> values, CancellationToken ct);

    /// <summary>Approve/reject the current pending step (assigned to the current user).
    /// On final approval, applies all configured impacts.</summary>
    Task<RequestInstance> DecideAsync(Guid requestInstanceId, bool approved, string? comment, CancellationToken ct);

    /// <summary>Return the request to the requester for changes (current approver action).</summary>
    Task<RequestInstance> ReturnAsync(Guid requestInstanceId, string? comment, CancellationToken ct);

    /// <summary>Cancel a request the current user owns (while still pending).</summary>
    Task<RequestInstance> CancelAsync(Guid requestInstanceId, CancellationToken ct);
}
