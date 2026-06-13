namespace HR.Domain.Enums;

/// <summary>System (built-in, ships ready) vs Dynamic (tenant-created via builder).</summary>
public enum RequestKind
{
    System = 1,
    Dynamic = 2,
}

public enum RequestApprovalStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Skipped = 4,
}

public enum RequestPermissionAction
{
    Submit = 1,
    Approve = 2,
    View = 3,
}

/// <summary>Attendance day state — written when leave/attendance-correction requests are approved.</summary>
public enum AttendanceStatus
{
    Present = 1,
    Absent = 2,
    OnLeave = 3,
    Holiday = 4,
    Weekend = 5,
    Late = 6,
    Remote = 7,
}
