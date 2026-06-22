using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Completion;

/// <summary>
/// One completion run per approved request (1:1). Holds the overall Completion Status — kept
/// distinct from the request's workflow status — plus timing and failure metadata. Its child
/// <see cref="CompletionEffect"/> rows record each effect's execution.
/// </summary>
public class CompletionRun : TenantEntity
{
    public Guid RequestInstanceId { get; set; }
    public Guid? WorkflowInstanceId { get; set; }

    public CompletionRunStatus Status { get; set; } = CompletionRunStatus.Pending;

    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? DurationMs { get; set; }
    public int Attempts { get; set; }

    public string? FailureReason { get; set; }

    /// <summary>The final approver who triggered completion.</summary>
    public Guid? FinalApproverUserId { get; set; }

    public ICollection<CompletionEffect> Effects { get; set; } = new List<CompletionEffect>();
}
