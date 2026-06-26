using System.Globalization;

namespace HR.Domain.Engines.Finance.Expressions;

public enum RuleValueKind
{
    Null = 0,
    Number = 1,
    Boolean = 2,
    Text = 3,
}

/// <summary>A dynamically-typed value flowing through the rule expression engine. A rule's facts,
/// intermediate values and results are all <see cref="RuleValue"/>s. Coercion rules are deliberately
/// explicit so a payroll calculation is reproducible and never silently does the wrong thing.</summary>
public readonly struct RuleValue : IEquatable<RuleValue>
{
    public RuleValueKind Kind { get; }
    private readonly decimal _number;
    private readonly bool _boolean;
    private readonly string? _text;

    private RuleValue(RuleValueKind kind, decimal number, bool boolean, string? text)
    {
        Kind = kind;
        _number = number;
        _boolean = boolean;
        _text = text;
    }

    public static readonly RuleValue Null = new(RuleValueKind.Null, 0m, false, null);
    public static RuleValue Number(decimal value) => new(RuleValueKind.Number, value, false, null);
    public static RuleValue Bool(bool value) => new(RuleValueKind.Boolean, 0m, value, null);
    public static RuleValue Text(string value) => new(RuleValueKind.Text, 0m, false, value ?? string.Empty);

    /// <summary>Wrap an arbitrary CLR fact value as a RuleValue (used when building the evaluation
    /// context from heterogeneous employee/period facts).</summary>
    public static RuleValue From(object? value) => value switch
    {
        null => Null,
        RuleValue rv => rv,
        bool b => Bool(b),
        decimal d => Number(d),
        int i => Number(i),
        long l => Number(l),
        short s => Number(s),
        double db => Number((decimal)db),
        float f => Number((decimal)f),
        string str => Text(str),
        _ => Text(value.ToString() ?? string.Empty),
    };

    public bool IsNull => Kind == RuleValueKind.Null;

    public decimal AsNumber() => Kind switch
    {
        RuleValueKind.Number => _number,
        RuleValueKind.Boolean => _boolean ? 1m : 0m,
        RuleValueKind.Text when decimal.TryParse(_text, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) => d,
        RuleValueKind.Text => throw new ExpressionException($"Cannot use text '{_text}' as a number."),
        _ => throw new ExpressionException("Cannot use a null value as a number."),
    };

    /// <summary>Truthiness: null → false, number → (≠ 0), bool → itself, text → non-empty.</summary>
    public bool AsBool() => Kind switch
    {
        RuleValueKind.Boolean => _boolean,
        RuleValueKind.Number => _number != 0m,
        RuleValueKind.Text => !string.IsNullOrEmpty(_text),
        _ => false,
    };

    public string AsText() => Kind switch
    {
        RuleValueKind.Text => _text ?? string.Empty,
        RuleValueKind.Number => _number.ToString(CultureInfo.InvariantCulture),
        RuleValueKind.Boolean => _boolean ? "true" : "false",
        _ => string.Empty,
    };

    public bool Equals(RuleValue other)
    {
        // Number compares numerically; otherwise compare within kind. Null equals only Null.
        if (Kind == RuleValueKind.Null || other.Kind == RuleValueKind.Null)
            return Kind == RuleValueKind.Null && other.Kind == RuleValueKind.Null;
        if (Kind == RuleValueKind.Number || other.Kind == RuleValueKind.Number)
            return TryNumeric(this, out var a) && TryNumeric(other, out var b) && a == b;
        if (Kind == RuleValueKind.Boolean || other.Kind == RuleValueKind.Boolean)
            return AsBool() == other.AsBool();
        return string.Equals(AsText(), other.AsText(), StringComparison.Ordinal);
    }

    private static bool TryNumeric(RuleValue v, out decimal result)
    {
        try { result = v.AsNumber(); return true; }
        catch (ExpressionException) { result = 0m; return false; }
    }

    public override bool Equals(object? obj) => obj is RuleValue other && Equals(other);
    public override int GetHashCode() => Kind switch
    {
        RuleValueKind.Number => _number.GetHashCode(),
        RuleValueKind.Boolean => _boolean.GetHashCode(),
        RuleValueKind.Text => _text?.GetHashCode(StringComparison.Ordinal) ?? 0,
        _ => 0,
    };

    public override string ToString() => Kind switch
    {
        RuleValueKind.Number => _number.ToString(CultureInfo.InvariantCulture),
        RuleValueKind.Boolean => _boolean ? "true" : "false",
        RuleValueKind.Text => $"\"{_text}\"",
        _ => "null",
    };
}
