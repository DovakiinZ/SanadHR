using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Modules.Employees.Entities;

/// <summary>A request to restore (reinstate) an employee who previously left the organization
/// (Terminated/Resigned). It runs through a lightweight approval chain (Manager → HR); on final
/// approval the employee is reactivated (Status → Active, termination data cleared). Self-contained
/// approval lifecycle, mirroring <see cref="TerminationSettlement"/>.</summary>
public class EmployeeRestoreRequest : TenantEntity
{
    public Guid EmployeeId { get; set; }

    /// <summary>HR's stated reason for reinstating the employee.</summary>
    public string? Reason { get; set; }

    public SettlementStatus Status { get; set; } = SettlementStatus.PendingApproval;
    /// <summary>1-based index of the approval step currently awaiting a decision.</summary>
    public int CurrentStep { get; set; } = 1;

    public Guid? RequestedByUserId { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }

    public ICollection<RestoreApprovalStep> ApprovalSteps { get; set; } = new List<RestoreApprovalStep>();
}

/// <summary>One step of a restore request's approval chain. Reuses the settlement approver roles +
/// statuses; the role determines the permission required to decide.</summary>
public class RestoreApprovalStep : TenantEntity
{
    public Guid EmployeeRestoreRequestId { get; set; }
    public EmployeeRestoreRequest? Request { get; set; }

    public int StepOrder { get; set; }
    public SettlementApproverRole Role { get; set; }
    public Guid? ApproverUserId { get; set; }

    public SettlementApprovalStepStatus Status { get; set; } = SettlementApprovalStepStatus.Pending;
    public Guid? DecidedByUserId { get; set; }
    public DateTime? DecidedAt { get; set; }
    public string? Comment { get; set; }
}
