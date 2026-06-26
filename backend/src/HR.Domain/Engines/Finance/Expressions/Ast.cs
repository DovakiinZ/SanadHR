namespace HR.Domain.Engines.Finance.Expressions;

/// <summary>Raised for any expression parse or evaluation error. Carries a human-readable message so it
/// can surface as a rule-validation failure in the UI.</summary>
public sealed class ExpressionException : Exception
{
    public ExpressionException(string message) : base(message) { }
}

public enum BinaryOp
{
    Add, Subtract, Multiply, Divide, Modulo,
    Equal, NotEqual, LessThan, LessThanOrEqual, GreaterThan, GreaterThanOrEqual,
    And, Or,
}

public enum UnaryOp
{
    Negate, Not,
}

/// <summary>An immutable abstract-syntax-tree node for a rule expression. The AST is what gets stored
/// (as JSON) and evaluated, guaranteeing a rule computes identically years later regardless of how the
/// source text was authored.</summary>
public abstract record Expr;

public sealed record LiteralExpr(RuleValue Value) : Expr;

public sealed record VariableExpr(string Name) : Expr;

public sealed record UnaryExpr(UnaryOp Op, Expr Operand) : Expr;

public sealed record BinaryExpr(BinaryOp Op, Expr Left, Expr Right) : Expr;

public sealed record FunctionCallExpr(string Name, IReadOnlyList<Expr> Arguments) : Expr;
