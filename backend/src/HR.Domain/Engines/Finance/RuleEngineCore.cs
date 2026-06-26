using HR.Domain.Engines.Finance.Expressions;
using HR.Domain.Engines.Finance.Graph;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Finance;

/// <summary>A rule reduced to its executable form: a unique output variable, an optional guard condition
/// and the value expression. Produced from a persisted Rule by compiling its source/AST; consumed by
/// <see cref="RuleEngineCore"/>. Pure data — no DB, no I/O.</summary>
public sealed record CompiledRule(
    string Code,
    string OutputComponentCode,
    PayComponentKind Kind,
    Expr? Condition,
    Expr Expression,
    int AuthoredSequence = 0);

/// <summary>The computed outcome of one rule within a run.</summary>
public sealed record ComponentResult(
    string Code,
    string ComponentCode,
    PayComponentKind Kind,
    decimal Amount,
    bool Applied);

/// <summary>The result of evaluating a whole rule set against one employee's facts: every component, the
/// order it ran in, and the derived gross/deduction/net totals.</summary>
public sealed record RuleSetEvaluation(
    IReadOnlyList<ComponentResult> Components,
    IReadOnlyList<string> ExecutionOrder,
    decimal GrossEarnings,
    decimal TotalDeductions,
    decimal NetAmount);

/// <summary>Pure evaluation of a configurable rule set. Builds the dependency graph from what each rule
/// reads vs writes, topologically orders the rules (no hardcoded sequence), then evaluates each rule in
/// order, feeding every output back into the context so later rules can consume it. Deterministic and
/// side-effect free, so a payroll computation is fully reproducible.</summary>
public static class RuleEngineCore
{
    /// <summary>Order rules by dependency. A rule depends on any other rule whose output variable it
    /// references in its condition or expression. Throws <see cref="DependencyCycleException"/> on cycles.</summary>
    public static IReadOnlyList<CompiledRule> Order(IReadOnlyCollection<CompiledRule> rules)
    {
        var graph = new DependencyGraph<CompiledRule>();
        // Stable insertion order: authored sequence then code.
        foreach (var rule in rules.OrderBy(r => r.AuthoredSequence).ThenBy(r => r.Code, StringComparer.OrdinalIgnoreCase))
            graph.AddNode(rule.Code, rule);

        foreach (var rule in rules)
        {
            var reads = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (rule.Condition is not null)
                foreach (var v in VariableExtractor.Extract(rule.Condition)) reads.Add(v);
            foreach (var v in VariableExtractor.Extract(rule.Expression)) reads.Add(v);
            foreach (var dep in reads) graph.AddDependency(rule.Code, dep);
        }

        return graph.TopologicalSort();
    }

    /// <summary>Evaluate the rule set against the supplied facts. The context is seeded with the facts and
    /// each rule's output is written back under its Code.</summary>
    public static RuleSetEvaluation Evaluate(
        IReadOnlyCollection<CompiledRule> rules,
        MutableEvaluationContext context,
        FunctionRegistry? functions = null)
    {
        var evaluator = new ExpressionEvaluator(functions);
        var ordered = Order(rules);

        var components = new List<ComponentResult>();
        decimal gross = 0m, deductions = 0m;

        foreach (var rule in ordered)
        {
            var applies = true;
            if (rule.Condition is not null)
                applies = evaluator.Evaluate(rule.Condition, context).AsBool();

            var amount = 0m;
            if (applies)
            {
                amount = evaluator.Evaluate(rule.Expression, context).AsNumber();
            }

            // Feed the result back so downstream rules can read it (0 when the guard failed).
            context.Set(rule.Code, RuleValue.Number(amount));

            components.Add(new ComponentResult(rule.Code, rule.OutputComponentCode, rule.Kind, amount, applies));

            if (applies)
            {
                switch (rule.Kind)
                {
                    case PayComponentKind.Earning: gross += amount; break;
                    case PayComponentKind.Deduction: deductions += amount; break;
                    // Contributions (employer-side) and Information components don't affect net.
                }
            }
        }

        var net = gross - deductions;
        return new RuleSetEvaluation(
            components,
            ordered.Select(r => r.Code).ToList(),
            Math.Round(gross, 2, MidpointRounding.AwayFromZero),
            Math.Round(deductions, 2, MidpointRounding.AwayFromZero),
            Math.Round(net, 2, MidpointRounding.AwayFromZero));
    }
}
