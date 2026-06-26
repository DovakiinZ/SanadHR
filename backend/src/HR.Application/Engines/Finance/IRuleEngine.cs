using HR.Domain.Engines.Finance;

namespace HR.Application.Engines.Finance;

/// <summary>The outcome of compiling/validating a rule-set version: whether every rule parses, the derived
/// execution order, and any errors (parse failures, dependency cycles).</summary>
public record RuleCompilationResult
{
    public bool IsValid { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = new List<string>();
    public IReadOnlyList<string> ExecutionOrder { get; init; } = new List<string>();
}

/// <summary>A rule-set version compiled once and reused across many employees in a run. Holds the compiled
/// rules + resolved execution order + function library, so evaluating each employee is just a context
/// pass — no repeated DB loads or re-parsing.</summary>
public interface IRuleSetEvaluator
{
    IReadOnlyList<string> ExecutionOrder { get; }
    RuleSetEvaluation Evaluate(IReadOnlyDictionary<string, object?> facts);
}

/// <summary>Loads a persisted rule-set version, compiles its rules, resolves their execution order from
/// the dependency graph and evaluates them against a set of facts. The pure math lives in
/// <see cref="RuleEngineCore"/>; this engine is the DB-backed bridge to it.</summary>
public interface IRuleEngine
{
    /// <summary>Compile and validate a rule-set version without evaluating it. Use before publishing.</summary>
    Task<RuleCompilationResult> ValidateAsync(Guid ruleSetVersionId, CancellationToken ct = default);

    /// <summary>Compile a rule-set version into a reusable evaluator (compile once, evaluate many).</summary>
    Task<IRuleSetEvaluator> CompileAsync(Guid ruleSetVersionId, CancellationToken ct = default);

    /// <summary>Evaluate a rule-set version against employee/period facts. Facts may be numbers, booleans
    /// or strings (e.g. {"BASIC": 10000m, "Overtime": 25m, "Department": "Sales"}).</summary>
    Task<RuleSetEvaluation> EvaluateAsync(
        Guid ruleSetVersionId,
        IReadOnlyDictionary<string, object?> facts,
        CancellationToken ct = default);
}
