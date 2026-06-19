namespace HR.Domain.Engines.FlowBuilder;

/// <summary>
/// The kind of a <see cref="WorkflowStep"/>. The builder's linked-list model
/// (RootStepId + NextStepIdSuccess/NextStepIdFailure) is type-agnostic; the
/// <see cref="WorkflowStep.Type"/> only decides which step handler runs.
/// </summary>
public enum WorkflowStepType
{
    /// <summary>Waits for a human decision (approve/reject). Blocking.</summary>
    Approval = 1,

    /// <summary>Performs a side effect (e.g. send an e-mail) and continues. Non-blocking.</summary>
    Action = 2,

    /// <summary>Evaluates a predicate against the request payload to pick a branch. Non-blocking.</summary>
    Condition = 3,

    /// <summary>Terminal step. Reaching it completes the request.</summary>
    End = 4
}

/// <summary>
/// Lifecycle status of a <see cref="WorkflowRequest"/>.
/// </summary>
public enum WorkflowRequestStatus
{
    /// <summary>Created but not yet started running.</summary>
    Pending = 1,

    /// <summary>Currently parked on a blocking step (e.g. awaiting an approval).</summary>
    InProgress = 2,

    /// <summary>Reached an End step / ran out of steps successfully.</summary>
    Completed = 3,

    /// <summary>Cancelled by a user before completion.</summary>
    Cancelled = 4,

    /// <summary>Terminated because an approval was rejected with no failure branch.</summary>
    Rejected = 5
}
