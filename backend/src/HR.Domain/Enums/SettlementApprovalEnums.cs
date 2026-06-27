namespace HR.Domain.Enums;

/// <summary>Approval lifecycle of a termination settlement. A settlement created by the (new) request
/// flow starts <see cref="PendingApproval"/>; legacy/immediate settlements are <see cref="Approved"/>.</summary>
public enum SettlementStatus
{
    Draft = 1,
    PendingApproval = 2,
    Approved = 3,
    Rejected = 4,
}

/// <summary>The role responsible for one step of the termination approval chain. Drives which permission
/// a user must hold to decide that step.</summary>
public enum SettlementApproverRole
{
    Manager = 1,
    HR = 2,
    Finance = 3,
}

public enum SettlementApprovalStepStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Skipped = 4,
}
