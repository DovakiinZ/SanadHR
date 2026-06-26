using HR.Application.Engines.Finance;
using HR.Domain.Engines.Finance;
using HR.Domain.Engines.Finance.Expressions;

namespace HR.Infrastructure.Engines.Finance;

/// <summary>A rule-set version compiled once (rules + function library + resolved order) and evaluated
/// many times — once per employee — without re-loading or re-parsing. The execution order is computed up
/// front from the dependency graph.</summary>
public sealed class RuleSetEvaluator : IRuleSetEvaluator
{
    private readonly IReadOnlyList<CompiledRule> _rules;
    private readonly FunctionRegistry _functions;

    public RuleSetEvaluator(IReadOnlyList<CompiledRule> rules, FunctionRegistry functions)
    {
        _rules = rules;
        _functions = functions;
        // Resolve (and validate) the execution order eagerly so cycles surface at compile time.
        ExecutionOrder = RuleEngineCore.Order(rules).Select(r => r.Code).ToList();
    }

    public IReadOnlyList<string> ExecutionOrder { get; }

    public RuleSetEvaluation Evaluate(IReadOnlyDictionary<string, object?> facts)
    {
        var context = MutableEvaluationContext.FromFacts(facts);
        return RuleEngineCore.Evaluate(_rules, context, _functions);
    }
}
