namespace HR.Application.Engines.Completion;

/// <summary>
/// The single orchestration layer that turns an approved request into real business actions.
/// It materializes the request's completion effects, executes them atomically through their
/// registered executors, and tracks status/audit — without knowing any module's internals.
/// </summary>
public interface ICompletionEngine
{
    Task<CompletionResult> ExecuteAsync(Guid requestInstanceId, CancellationToken ct);
}

/// <summary>Outcome of a completion run.</summary>
public sealed class CompletionResult
{
    public bool Success { get; init; }
    public Guid? RunId { get; init; }
    public int EffectCount { get; init; }
    public string? Error { get; init; }

    public static CompletionResult Ok(Guid runId, int effectCount)
        => new() { Success = true, RunId = runId, EffectCount = effectCount };

    public static CompletionResult Fail(Guid? runId, string error)
        => new() { Success = false, RunId = runId, Error = error };
}
