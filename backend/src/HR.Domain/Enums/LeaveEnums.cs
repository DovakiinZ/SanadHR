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
