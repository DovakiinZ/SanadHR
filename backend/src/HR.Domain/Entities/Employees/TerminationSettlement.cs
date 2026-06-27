using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Modules.Employees.Entities;

/// <summary>A persisted end-of-service settlement computed when an employee is terminated or resigns.
/// Snapshots the wage base, derived service length and each statutory component (gratuity, Art. 77
/// award, notice) so the figures are reproducible and auditable. The itemized basis lives in
/// <see cref="Items"/>.</summary>
public class TerminationSettlement : TenantEntity
{
    public Guid EmployeeId { get; set; }

    public DateTime HireDate { get; set; }
    public DateTime TerminationDate { get; set; }

    public TerminationScenario Scenario { get; set; }
    public ContractTermType ContractTermType { get; set; }

    public decimal MonthlyWage { get; set; }
    public decimal DailyWage { get; set; }
    public decimal ServiceYears { get; set; }
    public decimal EffectiveServiceDays { get; set; }
    public decimal UnpaidLeaveDays { get; set; }

    public decimal GratuityAmount { get; set; }
    public decimal Article77Award { get; set; }
    public decimal NoticeCompensation { get; set; }
    public decimal TotalAward { get; set; }
    public string Currency { get; set; } = "SAR";

    public Guid? ComputedByUserId { get; set; }
    public DateTime ComputedAt { get; set; }

    public string? Notes { get; set; }

    // ── Approval lifecycle (lightweight, self-contained on the settlement) ──
    /// <summary>Approval state. A settlement created via the termination-request flow starts
    /// <see cref="SettlementStatus.PendingApproval"/>; legacy immediate settlements are Approved.</summary>
    public SettlementStatus Status { get; set; } = SettlementStatus.Approved;
    /// <summary>1-based index of the approval step currently awaiting a decision.</summary>
    public int CurrentStep { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    /// <summary>The pending Expense created for the settlement payout on final approval.</summary>
    public Guid? ExpenseId { get; set; }
    /// <summary>The generated settlement PDF (StoredFile served via /api/files/{id}).</summary>
    public Guid? DocumentFileId { get; set; }

    public ICollection<TerminationSettlementItem> Items { get; set; } = new List<TerminationSettlementItem>();
    public ICollection<TerminationApprovalStep> ApprovalSteps { get; set; } = new List<TerminationApprovalStep>();
}

/// <summary>One step of a termination settlement's approval chain (Manager → HR → Finance). The role
/// determines the permission required to decide; <see cref="ApproverUserId"/> may pin a specific approver
/// (e.g. the employee's direct manager); otherwise any user holding the role's permission may decide.</summary>
public class TerminationApprovalStep : TenantEntity
{
    public Guid TerminationSettlementId { get; set; }
    public TerminationSettlement? Settlement { get; set; }

    public int StepOrder { get; set; }
    public SettlementApproverRole Role { get; set; }
    public Guid? ApproverUserId { get; set; }

    public SettlementApprovalStepStatus Status { get; set; } = SettlementApprovalStepStatus.Pending;
    public Guid? DecidedByUserId { get; set; }
    public DateTime? DecidedAt { get; set; }
    public string? Comment { get; set; }
}
