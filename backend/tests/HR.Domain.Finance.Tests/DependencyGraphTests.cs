using FluentAssertions;
using HR.Domain.Engines.Finance.Graph;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class DependencyGraphTests
{
    [Fact]
    public void Orders_dependencies_before_dependents()
    {
        var g = new DependencyGraph<string>();
        g.AddNode("NET", "NET");
        g.AddNode("GROSS", "GROSS");
        g.AddNode("BASIC", "BASIC");
        g.AddDependency("NET", "GROSS");
        g.AddDependency("GROSS", "BASIC");

        var order = g.TopologicalSort();

        order.Should().Equal("BASIC", "GROSS", "NET");
    }

    [Fact]
    public void Handles_diamond_dependencies()
    {
        var g = new DependencyGraph<string>();
        foreach (var n in new[] { "A", "B", "C", "D" }) g.AddNode(n, n);
        g.AddDependency("B", "A");
        g.AddDependency("C", "A");
        g.AddDependency("D", "B");
        g.AddDependency("D", "C");

        var order = g.TopologicalSort().ToList();

        order.Should().HaveCount(4);
        order.IndexOf("A").Should().BeLessThan(order.IndexOf("B"));
        order.IndexOf("A").Should().BeLessThan(order.IndexOf("C"));
        order.IndexOf("B").Should().BeLessThan(order.IndexOf("D"));
        order.IndexOf("C").Should().BeLessThan(order.IndexOf("D"));
    }

    [Fact]
    public void Unknown_dependencies_are_treated_as_external_facts()
    {
        var g = new DependencyGraph<string>();
        g.AddNode("BONUS", "BONUS");
        g.AddDependency("BONUS", "Salary"); // Salary is an external fact, not a node

        var order = g.TopologicalSort();

        order.Should().Equal("BONUS");
    }

    [Fact]
    public void Detects_cycles()
    {
        var g = new DependencyGraph<string>();
        g.AddNode("A", "A");
        g.AddNode("B", "B");
        g.AddDependency("A", "B");
        g.AddDependency("B", "A");

        var act = () => g.TopologicalSort();

        act.Should().Throw<DependencyCycleException>()
            .Which.Cycle.Should().Contain(new[] { "A", "B" });
    }

    [Fact]
    public void Ties_break_by_insertion_order_for_determinism()
    {
        var g = new DependencyGraph<string>();
        g.AddNode("X", "X");
        g.AddNode("Y", "Y");
        g.AddNode("Z", "Z");

        g.TopologicalSort().Should().Equal("X", "Y", "Z");
    }
}
