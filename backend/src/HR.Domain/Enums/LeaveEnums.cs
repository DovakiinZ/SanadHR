namespace HR.Domain.Enums;

/// <summary>Lifecycle state of an HR-managed leave record (distinct from the request lifecycle).</summary>
public enum LeaveRecordStatus
{
    Approved = 1,   // created from an approved leave request
    Assigned = 2,   // assigned directly by HR (no request)
    Canceled = 3,
    Edited = 4,
}

/// <summary>Where a leave record originated.</summary>
public enum LeaveRecordSource
{
    Request = 1,        // employee leave request → approved
    HRAssignment = 2,   // HR assigned directly
    Import = 3,
    System = 4,
}

/// <summary>Classifies a LeaveBalanceTransaction. Usage/Restoration are written by the leave
/// record lifecycle (deduction on approval, restore on cancel); Accrual/Forfeiture are written by
/// the leave accrual engine. Existing rows default to Usage.</summary>
public enum LeaveTransactionType
{
    Usage = 1,        // -days, deducted when leave is approved/assigned
    Accrual = 2,      // +days, periodic entitlement earned over service
    Adjustment = 3,   // manual +/- correction by HR
    Forfeiture = 4,   // -days, expired/capped entitlement removed
    Restoration = 5,  // +days, returned when a leave is canceled/edited
}
