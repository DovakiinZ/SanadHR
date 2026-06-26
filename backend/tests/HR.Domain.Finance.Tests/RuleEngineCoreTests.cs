using FluentAssertions;
using HR.Domain.Engines.Finance;
using HR.Domain.Engines.Finance.Expressions;
using HR.Domain.Enums;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class RuleEngineCoreTests
{
    private static CompiledRule Rule(string code, PayComponentKind kind, string expr, string? condition = null, int seq = 0)
        => new(
            code,
            code,
            kind,
            condition is null ? null : ExpressionParser.Parse(condition),
            ExpressionParser.Parse(expr),
            seq);

    [Fact]
    public void Computes_the_spec_example_bonus_when_conditions_met()
    {
        // IF Department == Sales AND Overtime > 20 THEN Bonus = Salary * 10%
        var rules = new[]
        {
            Rule("BONUS", PayComponentKind.Earning, "Salary * 0.10",
                condition: "Department == \"Sales\" AND Overtime > 20"),
        };

        var ctx = MutableEvaluationContext.FromFacts(new Dictionary<string, object?>
        {
            ["Department"] = "Sales",
            ["Overtime"] = 25m,
            ["Salary"] = 10000m,
        });

        var result = RuleEngineCore.Evaluate(rules, ctx);

        result.Components.Single().Amount.Should().Be(1000m);
        result.Components.Single().Applied.Should().BeTrue();
        result.GrossEarnings.Should().Be(1000m);
    }

    [Fact]
    public void Skips_bonus_when_condition_fails_and_records_zero()
    {
        var rules = new[]
        {
            Rule("BONUS", PayComponentKind.Earning, "Salary * 0.10",
                condition: "Department == \"Sales\" AND Overtime > 20"),
        };

        var ctx = MutableEvaluationContext.FromFacts(new Dictionary<string, object?>
        {
            ["Department"] = "Engineering",
            ["Overtime"] = 25m,
            ["Salary"] = 10000m,
        });

        var result = RuleEngineCore.Evaluate(rules, ctx);

        result.Components.Single().Applied.Should().BeFalse();
        result.Components.Single().Amount.Should().Be(0m);
        result.GrossEarnings.Should().Be(0m);
    }

    [Fact]
    public void Derives_execution_order_from_dependencies_regardless_of_authoring_order()
    {
        // Authored deliberately out of order: NET references GROSS, GROSS references BASIC/HOUSING/GOSI.
        var rules = new[]
        {
            Rule("NET", PayComponentKind.Information, "GROSS - GOSI", seq: 1),
            Rule("GROSS", PayComponentKind.Information, "BASIC + HOUSING", seq: 2),
            Rule("GOSI", PayComponentKind.Deduction, "PERCENT(BASIC + HOUSING, 9.75)", seq: 3),
            Rule("HOUSING", PayComponentKind.Earning, "BASIC * 0.25", seq: 4),
            Rule("BASIC", PayComponentKind.Earning, "BasicSalary", seq: 5),
        };

        var ctx = MutableEvaluationContext.FromFacts(new Dictionary<string, object?>
        {
            ["BasicSalary"] = 10000m,
        });

        var result = RuleEngineCore.Evaluate(rules, ctx);

        // BASIC must precede HOUSING/GROSS/GOSI which must precede NET.
        var order = result.ExecutionOrder.ToList();
        order.IndexOf("BASIC").Should().BeLessThan(order.IndexOf("HOUSING"));
        order.IndexOf("HOUSING").Should().BeLessThan(order.IndexOf("GROSS"));
        order.IndexOf("GROSS").Should().BeLessThan(order.IndexOf("NET"));
        order.IndexOf("GOSI").Should().BeLessThan(order.IndexOf("NET"));

        // BASIC 10000, HOUSING 2500 → GROSS 12500; GOSI = 9.75% of 12500 = 1218.75
        Value(result, "GROSS").Should().Be(12500m);
        Value(result, "GOSI").Should().Be(1218.75m);
        Value(result, "NET").Should().Be(11281.25m);
    }

    [Fact]
    public void Aggregates_gross_deductions_and_net_by_component_kind()
    {
        var rules = new[]
        {
            Rule("BASIC", PayComponentKind.Earning, "10000"),
            Rule("HOUSING", PayComponentKind.Earning, "2500"),
            Rule("GOSI", PayComponentKind.Deduction, "PERCENT(BASIC, 9.75)"),
            Rule("EMPLOYER_GOSI", PayComponentKind.Contribution, "PERCENT(BASIC, 11.75)"),
        };

        var result = RuleEngineCore.Evaluate(rules, new MutableEvaluationContext());

        result.GrossEarnings.Should().Be(12500m);        // earnings only
        result.TotalDeductions.Should().Be(975m);        // GOSI employee
        result.NetAmount.Should().Be(11525m);            // gross - deductions; contribution excluded
    }

    [Fact]
    public void Cyclic_rules_are_rejected()
    {
        var rules = new[]
        {
            Rule("A", PayComponentKind.Information, "B + 1"),
            Rule("B", PayComponentKind.Information, "A + 1"),
        };

        var act = () => RuleEngineCore.Evaluate(rules, new MutableEvaluationContext());

        act.Should().Throw<HR.Domain.Engines.Finance.Graph.DependencyCycleException>();
    }

    private static decimal Value(RuleSetEvaluation r, string code) =>
        r.Components.First(c => c.Code == code).Amount;
}
