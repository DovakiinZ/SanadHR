namespace HR.Application.Engines.Completion;

/// <summary>
/// What an executor returns on success. Describes the record it created/changed (for completion
/// tracking) and the before/after state (for the audit trail). Failures are signalled by throwing.
/// </summary>
public sealed class EffectExecutionResult
{
    public string? TargetEntityType { get; init; }
    public Guid? TargetRecordId { get; init; }
    public object? BeforeState { get; init; }
    public object? AfterState { get; init; }
    public string? Summary { get; init; }

    public static EffectExecutionResult Ok(
        string? targetEntityType = null,
        Guid? targetRecordId = null,
        object? before = null,
        object? after = null,
        string? summary = null)
        => new()
        {
            TargetEntityType = targetEntityType,
            TargetRecordId = targetRecordId,
            BeforeState = before,
            AfterState = after,
            Summary = summary,
        };
}
