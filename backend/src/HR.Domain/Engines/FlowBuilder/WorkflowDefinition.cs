using HR.Domain.Common;

namespace HR.Domain.Engines.FlowBuilder;

/// <summary>
/// A versioned, decoupled definition of an HR approval workflow (e.g. "Annual Leave Approval").
/// The graph is a linked list of <see cref="WorkflowStep"/>s entered through <see cref="RootStepId"/>;
/// each step then points at the next step for the success / failure branch.
/// </summary>
public class WorkflowDefinition : TenantEntity
{
    /// <summary>Stable machine code, unique per tenant (e.g. "annual-leave-approval").</summary>
    public string Code { get; set; } = null!;

    /// <summary>Human friendly name, e.g. "Annual Leave Approval".</summary>
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    /// <summary>Optimistic version counter, bumped on every structural save.</summary>
    public int Version { get; set; } = 1;

    /// <summary>Only active definitions can start new requests.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>The first step executed when a request starts. Null while the graph is empty.</summary>
    public Guid? RootStepId { get; set; }

    public ICollection<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
    public ICollection<WorkflowRequest> Requests { get; set; } = new List<WorkflowRequest>();
}
