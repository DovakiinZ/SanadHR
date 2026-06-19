using FluentAssertions;
using HR.Domain.Engines.FlowBuilder;
using HR.Modules.Workflows.Execution;
using Xunit;

namespace HR.Modules.Workflows.Tests;

public class WorkflowGraphValidatorTests
{
    private readonly WorkflowGraphValidator _validator = new();

    private static WorkflowGraphNode Node(Guid id, Guid? ok = null, Guid? fail = null,
        WorkflowStepType type = WorkflowStepType.Approval, string name = "step")
        => new(id, type, name, ok, fail);

    [Fact]
    public void Empty_graph_is_valid()
    {
        var result = _validator.Validate(null, Array.Empty<WorkflowGraphNode>());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Linear_graph_terminating_in_null_is_valid()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var steps = new[]
        {
            Node(a, ok: b),                                   // A -> B
            Node(b, ok: null, type: WorkflowStepType.End)     // B is terminal
        };

        var result = _validator.Validate(a, steps);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Direct_cycle_is_rejected()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var steps = new[]
        {
            Node(a, ok: b),
            Node(b, ok: a)   // B -> A closes a loop
        };

        var result = _validator.Validate(a, steps);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Circular reference"));
    }

    [Fact]
    public void Self_reference_is_rejected()
    {
        var a = Guid.NewGuid();
        var result = _validator.Validate(a, new[] { Node(a, ok: a, name: "Loopy") });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("points to itself"));
    }

    [Fact]
    public void Dangling_pointer_is_rejected()
    {
        var a = Guid.NewGuid();
        var ghost = Guid.NewGuid();
        var result = _validator.Validate(a, new[] { Node(a, ok: ghost, name: "Orphaned") });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("does not exist"));
    }

    [Fact]
    public void Missing_root_with_steps_is_rejected()
    {
        var a = Guid.NewGuid();
        var result = _validator.Validate(null, new[] { Node(a, type: WorkflowStepType.End) });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Root step is not set"));
    }

    [Fact]
    public void Unreachable_step_is_a_warning_not_an_error()
    {
        var a = Guid.NewGuid();
        var island = Guid.NewGuid();
        var steps = new[]
        {
            Node(a, type: WorkflowStepType.End),       // reachable root, terminal
            Node(island, type: WorkflowStepType.End)   // disconnected
        };

        var result = _validator.Validate(a, steps);

        result.IsValid.Should().BeTrue();
        result.Warnings.Should().Contain(w => w.Contains("unreachable"));
    }
}
