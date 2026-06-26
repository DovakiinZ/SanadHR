namespace HR.Domain.Engines.Finance.Expressions;

/// <summary>Resolves variable names to values during expression evaluation. A context typically holds an
/// employee's facts for a pay period (basic salary, overtime hours, department, …) plus the outputs of
/// rules already evaluated this run.</summary>
public interface IEvaluationContext
{
    bool TryResolve(string name, out RuleValue value);
}

/// <summary>A mutable, case-insensitive variable bag. Rule outputs are written back so later rules in the
/// dependency order can read them (e.g. NET depends on GROSS depends on BASIC).</summary>
public sealed class MutableEvaluationContext : IEvaluationContext
{
    private readonly Dictionary<string, RuleValue> _values =
        new(StringComparer.OrdinalIgnoreCase);

    public MutableEvaluationContext() { }

    public MutableEvaluationContext(IEnumerable<KeyValuePair<string, RuleValue>> seed)
    {
        foreach (var kv in seed) _values[kv.Key] = kv.Value;
    }

    public static MutableEvaluationContext FromFacts(IEnumerable<KeyValuePair<string, object?>> facts)
    {
        var ctx = new MutableEvaluationContext();
        foreach (var kv in facts) ctx.Set(kv.Key, RuleValue.From(kv.Value));
        return ctx;
    }

    public void Set(string name, RuleValue value) => _values[name] = value;

    public bool TryResolve(string name, out RuleValue value) => _values.TryGetValue(name, out value);

    public IReadOnlyDictionary<string, RuleValue> Values => _values;
}
