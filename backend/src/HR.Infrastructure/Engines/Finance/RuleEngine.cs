using HR.Application.Engines.Finance;
using HR.Domain.Engines.Finance;
using HR.Domain.Engines.Finance.Expressions;
using HR.Domain.Engines.Finance.Entities;
using HR.Domain.Engines.Finance.Graph;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Engines.Finance;

/// <summary>DB-backed bridge to the pure <see cref="RuleEngineCore"/>. Loads a rule-set version and its
/// rules (plus any tenant-defined formula functions), compiles each expression, then validates or
/// evaluates via the core. Execution order is always derived from the dependency graph — never hardcoded.</summary>
public sealed class RuleEngine : IRuleEngine
{
    private readonly ApplicationDbContext _db;

    public RuleEngine(ApplicationDbContext db) => _db = db;

    public async Task<RuleCompilationResult> ValidateAsync(Guid ruleSetVersionId, CancellationToken ct = default)
    {
        var rules = await LoadRulesAsync(ruleSetVersionId, ct);
        var errors = new List<string>();
        var compiled = new List<CompiledRule>();

        foreach (var rule in rules)
        {
            try { compiled.Add(Compile(rule)); }
            catch (ExpressionException ex) { errors.Add($"Rule '{rule.Code}': {ex.Message}"); }
        }

        if (errors.Count > 0)
            return new RuleCompilationResult { IsValid = false, Errors = errors };

        try
        {
            var ordered = RuleEngineCore.Order(compiled);
            return new RuleCompilationResult
            {
                IsValid = true,
                ExecutionOrder = ordered.Select(r => r.Code).ToList(),
            };
        }
        catch (DependencyCycleException ex)
        {
            return new RuleCompilationResult { IsValid = false, Errors = new[] { ex.Message } };
        }
    }

    public async Task<IRuleSetEvaluator> CompileAsync(Guid ruleSetVersionId, CancellationToken ct = default)
    {
        var rules = await LoadRulesAsync(ruleSetVersionId, ct);
        var compiled = rules.Select(Compile).ToList();
        var functions = await BuildFunctionRegistryAsync(ct);
        return new RuleSetEvaluator(compiled, functions);
    }

    public async Task<RuleSetEvaluation> EvaluateAsync(
        Guid ruleSetVersionId,
        IReadOnlyDictionary<string, object?> facts,
        CancellationToken ct = default)
    {
        var evaluator = await CompileAsync(ruleSetVersionId, ct);
        return evaluator.Evaluate(facts);
    }

    private async Task<List<Rule>> LoadRulesAsync(Guid ruleSetVersionId, CancellationToken ct)
    {
        var rules = await _db.Set<Rule>()
            .AsNoTracking()
            .Where(r => r.RuleSetVersionId == ruleSetVersionId && r.IsActive)
            .ToListAsync(ct);
        if (rules.Count == 0)
            throw new InvalidOperationException($"Rule-set version {ruleSetVersionId} has no active rules.");
        return rules;
    }

    private static CompiledRule Compile(Rule rule)
    {
        var expression = CompileExpression(rule.ExpressionAstJson, rule.ExpressionText)
            ?? throw new ExpressionException("Expression is empty.");
        var condition = CompileExpression(rule.ConditionAstJson, rule.ConditionText);
        return new CompiledRule(
            rule.Code,
            string.IsNullOrWhiteSpace(rule.OutputComponentCode) ? rule.Code : rule.OutputComponentCode,
            rule.Kind,
            condition,
            expression,
            rule.Sequence);
    }

    /// <summary>Prefer the frozen compiled AST (reproducible); fall back to parsing the source text.</summary>
    private static Expr? CompileExpression(string? astJson, string? text)
    {
        if (!string.IsNullOrWhiteSpace(astJson)) return AstJson.Deserialize(astJson);
        if (!string.IsNullOrWhiteSpace(text)) return ExpressionParser.Parse(text);
        return null;
    }

    /// <summary>Built-in functions plus any published tenant-defined formula functions.</summary>
    private async Task<FunctionRegistry> BuildFunctionRegistryAsync(CancellationToken ct)
    {
        var registry = FunctionRegistry.CreateDefault();
        var formulas = await _db.Set<FormulaFunction>()
            .AsNoTracking()
            .Where(f => f.Status == HR.Domain.Enums.VersionStatus.Published)
            .ToListAsync(ct);

        foreach (var formula in formulas)
        {
            var body = CompileExpression(formula.ExpressionAstJson, formula.ExpressionText);
            if (body is null) continue;
            var parameters = formula.ParametersCsv
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            registry.Register(formula.Name, args =>
            {
                // Custom functions are pure of outer context: bind args to declared params only.
                var local = new MutableEvaluationContext();
                for (var i = 0; i < parameters.Length && i < args.Count; i++)
                    local.Set(parameters[i], args[i]);
                // Built-ins only inside a formula body, to keep evaluation total and cycle-free.
                return new ExpressionEvaluator().Evaluate(body, local);
            });
        }

        return registry;
    }
}
