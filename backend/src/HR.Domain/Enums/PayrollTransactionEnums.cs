namespace HR.Domain.Enums;

/// <summary>Whether a payroll transaction adds to or subtracts from net pay. Sign is implied by Kind;
/// the stored Amount is always non-negative.</summary>
public enum PayrollTransactionKind
{
    Addition = 1,
    Deduction = 2,
}

/// <summary>The full lifecycle of an addition/deduction record. Draft→PendingApproval→Approved are wired in
/// sub-project 2A; Posted/CarriedForward/Reversed are reached by the payroll engine in 2B.</summary>
public enum PayrollTransactionStatus
{
    Draft = 0,
    PendingApproval = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4,
    CarriedForward = 5,
    Posted = 6,
    Reversed = 7,
}
