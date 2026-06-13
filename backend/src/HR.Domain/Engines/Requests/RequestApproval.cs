using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Requests;

/// <summary>One step in a request's approval chain (resolved from the linked workflow at submit).</summary>
public class RequestApproval : BaseEntity
{
    public Guid RequestInstanceId { get; set; }
    public int StepOrder { get; set; }
    public string StepNameAr { get; set; } = null!;
    public string StepNameEn { get; set; } = null!;
    public ApproverType ApproverType { get; set; }

    public Guid? AssignedToUserId { get; set; }
    public RequestApprovalStatus Status { get; set; } = RequestApprovalStatus.Pending;
    public string? Comment { get; set; }
    public Guid? DecidedByUserId { get; set; }
    public DateTime? DecidedAt { get; set; }

    public RequestInstance RequestInstance { get; set; } = null!;
}
