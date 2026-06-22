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
    // Completion lifecycle: after final approval the Completion Engine runs the effects.
    // Approved stays the success terminal (so approved-leave/overlap queries are unaffected);
    // the detailed Completion Status lives on CompletionRun. CompletionFailed flags an approved
    // request whose effects were rolled back and needs support attention.
    CompletionFailed = 10,
}
