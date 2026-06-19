using HR.Domain.Common;

namespace HR.Domain.Engines.FlowBuilder;

/// <summary>
/// A single node in a <see cref="WorkflowDefinition"/>. Transitions are modelled as soft pointers
/// (<see cref="NextStepIdSuccess"/> / <see cref="NextStepIdFailure"/>) rather than FK relationships,
/// so the graph can be edited freely in the builder without EF cascade constraints. A null pointer
/// means "this branch ends here".
/// </summary>
public class WorkflowStep : TenantEntity
{
    public Guid DefinitionId { get; set; }
    public WorkflowDefinition Definition { get; set; } = null!;

    public WorkflowStepType Type { get; set; }

    /// <summary>Display label shown on the timeline node.</summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Step-specific configuration, stored as JSON (jsonb). Shape depends on <see cref="Type"/>:
    /// Approval => { approverRole, approverUserId }, Action => { actionType, toEmail, subject, body },
    /// Condition => { field, operator, value }.
    /// </summary>
    public string Config { get; set; } = "{}";

    /// <summary>Next step when the step succeeds / is approved / condition is true. Null = end of branch.</summary>
    public Guid? NextStepIdSuccess { get; set; }

    /// <summary>Next step when the step fails / is rejected / condition is false. Null = end of branch.</summary>
    public Guid? NextStepIdFailure { get; set; }

    /// <summary>Vertical position in the timeline builder (drives optimistic reordering).</summary>
    public int SortOrder { get; set; }
}
