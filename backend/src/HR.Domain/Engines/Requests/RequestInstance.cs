using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Requests;

/// <summary>A submitted request — links the employee, the form submission, the workflow run.</summary>
public class RequestInstance : TenantEntity
{
    public Guid RequestTypeId { get; set; }
    public string RequestNumber { get; set; } = null!;     // human reference, e.g. REQ-2026-000123
    public Guid EmployeeId { get; set; }
    public Guid FormSubmissionId { get; set; }
    public Guid? WorkflowInstanceId { get; set; }

    public RequestStatus Status { get; set; } = RequestStatus.Draft;
    public int CurrentStepOrder { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? DecidedAt { get; set; }
    public string? DecisionNote { get; set; }

    public Guid? GeneratedDocumentId { get; set; }

    // Leave snapshot (set for leave requests so impacts/print are object-driven, not re-parsed)
    public Guid? LeaveTypeId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? DaysCount { get; set; }

    public RequestType RequestType { get; set; } = null!;
    public ICollection<RequestApproval> Approvals { get; set; } = new List<RequestApproval>();
    public ICollection<RequestStatusHistory> History { get; set; } = new List<RequestStatusHistory>();
}
