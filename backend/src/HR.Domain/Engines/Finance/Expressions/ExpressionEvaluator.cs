namespace HR.Domain.Engines.Finance.Expressions;

/// <summary>Evaluates an <see cref="Expr"/> AST against an <see cref="IEvaluationContext"/>. Pure and
/// deterministic: the same AST + context + function registry always yields the same result, which is the
/// reproducibility guarantee payroll needs.</summary>
public sealed class ExpressionEvaluator
{
    private readonly FunctionRegistry _functions;

    public ExpressionEvaluator(FunctionRegistry? functions = null)
    {
        _functions = functions ?? FunctionRegistry.CreateDefault();
    }

    public RuleValue Evaluate(Expr expr, IEvaluationContext context) => expr switch
    {
        LiteralExpr l => l.Value,
        VariableExpr v => context.TryResolve(v.Name, out var value)
            ? value
            : throw new ExpressionException($"Unknown variable '{v.Name}'."),
        UnaryExpr u => EvaluateUnary(u, context),
        BinaryExpr b => EvaluateBinary(b, context),
        FunctionCallExpr f => EvaluateFunction(f, context),
        _ => throw new ExpressionException($"Unsupported expression node '{expr.GetType().Name}'."),
    };

    private RuleValue EvaluateUnary(UnaryExpr u, IEvaluationContext context)
    {
        var operand = Evaluate(u.Operand, context);
        return u.Op switch
        {
            UnaryOp.Negate => RuleValue.Number(-operand.AsNumber()),
            UnaryOp.Not => RuleValue.Bool(!operand.AsBool()),
            _ => throw new ExpressionException($"Unsupported unary operator '{u.Op}'."),
        };
    }

    private RuleValue EvaluateBinary(BinaryExpr b, IEvaluationContext context)
    {
        // Short-circuit logical operators.
        if (b.Op == BinaryOp.And)
            return RuleValue.Bool(Evaluate(b.Left, context).AsBool() && Evaluate(b.Right, context).AsBool());
        if (b.Op == BinaryOp.Or)
            return RuleValue.Bool(Evaluate(b.Left, context).AsBool() || Evaluate(b.Right, context).AsBool());

        var left = Evaluate(b.Left, context);
        var right = Evaluate(b.Right, context);

        switch (b.Op)
        {
            case BinaryOp.Add: return RuleValue.Number(left.AsNumber() + right.AsNumber());
            case BinaryOp.Subtract: return RuleValue.Number(left.AsNumber() - right.AsNumber());
            case BinaryOp.Multiply: return RuleValue.Number(left.AsNumber() * right.AsNumber());
            case BinaryOp.Divide:
                var divisor = right.AsNumber();
                if (divisor == 0m) throw new ExpressionException("Division by zero.");
                return RuleValue.Number(left.AsNumber() / divisor);
            case BinaryOp.Modulo:
                var mod = right.AsNumber();
                if (mod == 0m) throw new ExpressionException("Modulo by zero.");
                return RuleValue.Number(left.AsNumber() % mod);
            case BinaryOp.Equal: return RuleValue.Bool(left.Equals(right));
            case BinaryOp.NotEqual: return RuleValue.Bool(!left.Equals(right));
            case BinaryOp.LessThan: return RuleValue.Bool(left.AsNumber() < right.AsNumber());
            case BinaryOp.LessThanOrEqual: return RuleValue.Bool(left.AsNumber() <= right.AsNumber());
            case BinaryOp.GreaterThan: return RuleValue.Bool(left.AsNumber() > right.AsNumber());
            case BinaryOp.GreaterThanOrEqual: return RuleValue.Bool(left.AsNumber() >= right.AsNumber());
            default: throw new ExpressionException($"Unsupported binary operator '{b.Op}'.");
        }
    }

    private RuleValue EvaluateFunction(FunctionCallExpr f, IEvaluationContext context)
    {
        if (!_functions.TryGet(f.Name, out var function))
            throw new ExpressionException($"Unknown function '{f.Name}'.");
        var args = new RuleValue[f.Arguments.Count];
        for (var i = 0; i < f.Arguments.Count; i++) args[i] = Evaluate(f.Arguments[i], context);
        return function(args);
    }
}
