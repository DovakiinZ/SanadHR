namespace HR.Domain.Enums;

public enum RequestStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4,
    // Request Center lifecycle (added; existing values preserved for back-compat)
    Draft = 5,
    Submitted = 6,
    InProgress = 7,
    Returned = 8,
}
