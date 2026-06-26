namespace HR.Domain.Engines.Finance.Expressions;

/// <summary>A named function callable from rule expressions. Receives already-evaluated argument values.
/// Custom, tenant-defined functions (the reusable "formula library") are registered here at runtime
/// alongside the built-ins.</summary>
public delegate RuleValue RuleFunction(IReadOnlyList<RuleValue> args);

/// <summary>The set of functions available to an evaluator. Starts from the built-in library and can be
/// extended with tenant-defined reusable formulas.</summary>
public sealed class FunctionRegistry
{
    private readonly Dictionary<string, RuleFunction> _functions =
        new(StringComparer.OrdinalIgnoreCase);

    public FunctionRegistry Register(string name, RuleFunction function)
    {
        _functions[name] = function;
        return this;
    }

    public bool TryGet(string name, out RuleFunction function) => _functions.TryGetValue(name, out function!);

    /// <summary>A fresh registry seeded with the built-in functions.</summary>
    public static FunctionRegistry CreateDefault()
    {
        var r = new FunctionRegistry();

        r.Register("IF", args =>
        {
            Arity("IF", args, 3);
            return args[0].AsBool() ? args[1] : args[2];
        });
        r.Register("AND", args =>
        {
            AtLeast("AND", args, 1);
            return RuleValue.Bool(args.All(a => a.AsBool()));
        });
        r.Register("OR", args =>
        {
            AtLeast("OR", args, 1);
            return RuleValue.Bool(args.Any(a => a.AsBool()));
        });
        r.Register("NOT", args =>
        {
            Arity("NOT", args, 1);
            return RuleValue.Bool(!args[0].AsBool());
        });
        r.Register("MIN", args =>
        {
            AtLeast("MIN", args, 1);
            return RuleValue.Number(args.Min(a => a.AsNumber()));
        });
        r.Register("MAX", args =>
        {
            AtLeast("MAX", args, 1);
            return RuleValue.Number(args.Max(a => a.AsNumber()));
        });
        r.Register("ROUND", args =>
        {
            if (args.Count is < 1 or > 2) throw new ExpressionException("ROUND expects 1 or 2 arguments.");
            var digits = args.Count == 2 ? (int)args[1].AsNumber() : 2;
            return RuleValue.Number(Math.Round(args[0].AsNumber(), digits, MidpointRounding.AwayFromZero));
        });
        r.Register("ABS", args => { Arity("ABS", args, 1); return RuleValue.Number(Math.Abs(args[0].AsNumber())); });
        r.Register("FLOOR", args => { Arity("FLOOR", args, 1); return RuleValue.Number(Math.Floor(args[0].AsNumber())); });
        r.Register("CEIL", args => { Arity("CEIL", args, 1); return RuleValue.Number(Math.Ceiling(args[0].AsNumber())); });
        r.Register("CLAMP", args =>
        {
            Arity("CLAMP", args, 3);
            var v = args[0].AsNumber();
            var lo = args[1].AsNumber();
            var hi = args[2].AsNumber();
            return RuleValue.Number(Math.Min(Math.Max(v, lo), hi));
        });
        r.Register("COALESCE", args =>
        {
            AtLeast("COALESCE", args, 1);
            foreach (var a in args) if (!a.IsNull) return a;
            return RuleValue.Null;
        });
        r.Register("PERCENT", args =>
        {
            // PERCENT(value, pct) → value * pct / 100
            Arity("PERCENT", args, 2);
            return RuleValue.Number(args[0].AsNumber() * args[1].AsNumber() / 100m);
        });

        return r;
    }

    private static void Arity(string name, IReadOnlyList<RuleValue> args, int count)
    {
        if (args.Count != count)
            throw new ExpressionException($"{name} expects {count} argument(s), got {args.Count}.");
    }

    private static void AtLeast(string name, IReadOnlyList<RuleValue> args, int count)
    {
        if (args.Count < count)
            throw new ExpressionException($"{name} expects at least {count} argument(s).");
    }
}
