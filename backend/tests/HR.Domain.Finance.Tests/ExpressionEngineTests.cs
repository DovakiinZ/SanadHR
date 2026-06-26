using FluentAssertions;
using HR.Domain.Engines.Finance.Expressions;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class ExpressionEngineTests
{
    private static decimal EvalNum(string expr, params (string, object?)[] facts)
    {
        var ast = ExpressionParser.Parse(expr);
        var ctx = MutableEvaluationContext.FromFacts(
            facts.Select(f => new KeyValuePair<string, object?>(f.Item1, f.Item2)));
        return new ExpressionEvaluator().Evaluate(ast, ctx).AsNumber();
    }

    private static bool EvalBool(string expr, params (string, object?)[] facts)
    {
        var ast = ExpressionParser.Parse(expr);
        var ctx = MutableEvaluationContext.FromFacts(
            facts.Select(f => new KeyValuePair<string, object?>(f.Item1, f.Item2)));
        return new ExpressionEvaluator().Evaluate(ast, ctx).AsBool();
    }

    [Theory]
    [InlineData("2 + 3 * 4", 14)]      // precedence
    [InlineData("(2 + 3) * 4", 20)]    // parentheses
    [InlineData("10 - 2 - 3", 5)]      // left-assoc
    [InlineData("20 / 4 / 5", 1)]
    [InlineData("17 % 5", 2)]
    [InlineData("-5 + 8", 3)]          // unary minus
    public void Arithmetic_respects_precedence_and_associativity(string expr, int expected)
    {
        EvalNum(expr).Should().Be(expected);
    }

    [Theory]
    [InlineData("5 > 3", true)]
    [InlineData("5 <= 5", true)]
    [InlineData("3 == 3", true)]
    [InlineData("3 != 4", true)]
    [InlineData("2 > 3 OR 4 > 1", true)]
    [InlineData("2 > 3 AND 4 > 1", false)]
    [InlineData("NOT (2 > 3)", true)]
    [InlineData("!(1 == 2)", true)]
    public void Logical_and_comparison_operators_work(string expr, bool expected)
    {
        EvalBool(expr).Should().Be(expected);
    }

    [Fact]
    public void Variables_resolve_from_context()
    {
        EvalNum("Basic + Housing", ("Basic", 10000m), ("Housing", 2500m)).Should().Be(12500m);
    }

    [Fact]
    public void String_equality_supports_categorical_facts()
    {
        EvalBool("Department == \"Sales\"", ("Department", "Sales")).Should().BeTrue();
        EvalBool("Department == \"Sales\"", ("Department", "Engineering")).Should().BeFalse();
    }

    [Theory]
    [InlineData("IF(Overtime > 20, Salary * 0.10, 0)", 25, 10000, 1000)]
    [InlineData("IF(Overtime > 20, Salary * 0.10, 0)", 10, 10000, 0)]
    public void If_function_branches(string expr, int overtime, int salary, int expected)
    {
        EvalNum(expr, ("Overtime", (decimal)overtime), ("Salary", (decimal)salary))
            .Should().Be(expected);
    }

    [Theory]
    [InlineData("MIN(5, 3, 9)", 3)]
    [InlineData("MAX(5, 3, 9)", 9)]
    [InlineData("ROUND(3.14159, 2)", 3.14)]
    [InlineData("ABS(-7)", 7)]
    [InlineData("CLAMP(15, 0, 10)", 10)]
    [InlineData("PERCENT(2000, 9.75)", 195)]
    public void Builtin_functions(string expr, double expected)
    {
        EvalNum(expr).Should().Be((decimal)expected);
    }

    [Fact]
    public void Division_by_zero_throws()
    {
        var act = () => EvalNum("10 / 0");
        act.Should().Throw<ExpressionException>().WithMessage("*Division by zero*");
    }

    [Fact]
    public void Unknown_variable_throws()
    {
        var act = () => EvalNum("Foo + 1");
        act.Should().Throw<ExpressionException>().WithMessage("*Unknown variable*");
    }

    [Fact]
    public void Unknown_function_throws()
    {
        var act = () => EvalNum("BOGUS(1)");
        act.Should().Throw<ExpressionException>().WithMessage("*Unknown function*");
    }

    [Fact]
    public void Ast_json_round_trips_and_evaluates_identically()
    {
        const string source = "IF(Department == \"Sales\" AND Overtime > 20, Salary * 0.10, 0)";
        var ast = ExpressionParser.Parse(source);
        var json = AstJson.Serialize(ast);
        var restored = AstJson.Deserialize(json);

        var ctx = MutableEvaluationContext.FromFacts(new Dictionary<string, object?>
        {
            ["Department"] = "Sales",
            ["Overtime"] = 25m,
            ["Salary"] = 12000m,
        });
        var evaluator = new ExpressionEvaluator();
        evaluator.Evaluate(restored, ctx).AsNumber()
            .Should().Be(evaluator.Evaluate(ast, ctx).AsNumber())
            .And.Be(1200m);
    }

    [Fact]
    public void Validation_reports_parse_errors()
    {
        ExpressionParser.TryValidate("1 + ").Should().NotBeNull();
        ExpressionParser.TryValidate("1 + 2").Should().BeNull();
    }
}
