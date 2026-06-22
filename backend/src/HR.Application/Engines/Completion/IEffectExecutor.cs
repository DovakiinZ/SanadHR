namespace HR.Application.Engines.Completion;

/// <summary>
/// Polymorphic completion-effect contract. Each business module implements one executor per
/// effect it owns (e.g. "Leave.CreateApprovedLeave"). The Completion Engine resolves an executor
/// by <see cref="EffectType"/> and never references modules directly — new modules just register
/// new executors. Executors mutate their own module's data and return a result describing the
/// target record + before/after state for the audit trail. On failure they THROW: the engine
/// catches it, rolls back the whole completion transaction, and records the failure.
/// </summary>
public interface IEffectExecutor
{
    /// <summary>The effect type this executor handles, e.g. "Leave.CreateApprovedLeave".</summary>
    string EffectType { get; }

    /// <summary>Executor version, recorded on the completion + audit trail for traceability.</summary>
    string Version => "1.0";

    Task<EffectExecutionResult> ExecuteAsync(EffectContext context, CancellationToken ct);
}
