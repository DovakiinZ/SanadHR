using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Completion;

/// <summary>
/// One "intent to change" within a completion run: the effect type, its structured JSONB payload,
/// and the full execution record (status, attempts, timing, target record, result, error). The
/// Completion Engine writes these; modules never touch them.
/// </summary>
public class CompletionEffect : TenantEntity
{
    public Guid CompletionRunId { get; set; }
    public Guid RequestInstanceId { get; set; }

    public string EffectType { get; set; } = null!;
    public int Sequence { get; set; }

    /// <summary>Structured intent payload (JSONB).</summary>
    public string Payload { get; set; } = "{}";

    public CompletionEffectStatus Status { get; set; } = CompletionEffectStatus.Pending;
    public int Attempts { get; set; }

    public DateTime? ExecutedAt { get; set; }
    public int? DurationMs { get; set; }

    public string? ExecutorName { get; set; }
    public string? ExecutorVersion { get; set; }

    // What the effect created / changed (for the timeline + drill-down).
    public string? TargetEntityType { get; set; }
    public Guid? TargetRecordId { get; set; }

    /// <summary>Human/machine summary of the result (JSONB or text).</summary>
    public string? ResultSummary { get; set; }

    public string? FailureReason { get; set; }

    public CompletionRun Run { get; set; } = null!;
}
