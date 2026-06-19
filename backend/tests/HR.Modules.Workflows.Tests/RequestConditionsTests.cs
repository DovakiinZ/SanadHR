using FluentAssertions;
using HR.Modules.Platform.Services.Requests;
using Xunit;

namespace HR.Modules.Workflows.Tests;

/// <summary>
/// The no-code condition evaluator that decides whether an approval step joins the chain
/// (e.g. "Leave Days &gt; 5 → require HR"). Pure logic, no DB.
/// </summary>
public class RequestConditionsTests
{
    private static Dictionary<string, string?> Ctx(params (string Key, string? Val)[] kv)
        => kv.ToDictionary(x => x.Key, x => x.Val, StringComparer.OrdinalIgnoreCase);

    [Theory]
    [InlineData("6", "gt", "5", true)]
    [InlineData("5", "gt", "5", false)]
    [InlineData("5", "gte", "5", true)]
    [InlineData("4", "lt", "5", true)]
    [InlineData("5", "eq", "5", true)]
    [InlineData("5", "neq", "6", true)]
    public void Numeric_operators(string actual, string op, string expected, bool result)
        => RequestConditions.Evaluate(actual, op, expected).Should().Be(result);

    [Fact]
    public void String_equality_is_case_insensitive_for_entity_ids()
    {
        var id = "A1B2C3D4";
        RequestConditions.Evaluate(id.ToLower(), "eq", id).Should().BeTrue();
        RequestConditions.Evaluate("other", "neq", id).Should().BeTrue();
        RequestConditions.Evaluate("haystack-value", "contains", "stack").Should().BeTrue();
    }

    [Fact]
    public void Empty_conditions_always_match()
        => RequestConditions.Met(new List<StepConditionConfig>(), Ctx()).Should().BeTrue();

    [Fact]
    public void All_conditions_must_hold_AND_semantics()
    {
        var ctx = Ctx(("leaveDays", "7"), ("department", "FIN"));
        var pass = new List<StepConditionConfig>
        {
            new() { Field = "leaveDays", Operator = "gt", Value = "5" },
            new() { Field = "department", Operator = "eq", Value = "FIN" },
        };
        RequestConditions.Met(pass, ctx).Should().BeTrue();

        var fail = new List<StepConditionConfig>
        {
            new() { Field = "leaveDays", Operator = "gt", Value = "5" },
            new() { Field = "department", Operator = "eq", Value = "HR" }, // not met → step skipped
        };
        RequestConditions.Met(fail, ctx).Should().BeFalse();
    }

    [Fact]
    public void Missing_context_value_fails_the_condition()
    {
        var conds = new List<StepConditionConfig> { new() { Field = "amount", Operator = "gt", Value = "1000" } };
        RequestConditions.Met(conds, Ctx()).Should().BeFalse();
    }
}
