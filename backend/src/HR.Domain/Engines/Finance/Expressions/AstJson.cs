using System.Text.Json;
using System.Text.Json.Nodes;

namespace HR.Domain.Engines.Finance.Expressions;

/// <summary>Serializes an expression AST to/from a compact, stable JSON form for storage in jsonb
/// columns. Persisting the compiled AST (not just the source text) means a rule's exact semantics are
/// frozen and reproducible even if the parser or function library evolves later.</summary>
public static class AstJson
{
    public static string Serialize(Expr expr) => ToNode(expr).ToJsonString();

    public static Expr Deserialize(string json)
    {
        JsonNode? node;
        try { node = JsonNode.Parse(json); }
        catch (JsonException ex) { throw new ExpressionException($"Invalid AST JSON: {ex.Message}"); }
        if (node is null) throw new ExpressionException("Empty AST JSON.");
        return FromNode(node);
    }

    private static JsonObject ToNode(Expr expr)
    {
        switch (expr)
        {
            case LiteralExpr l:
                var lit = new JsonObject { ["t"] = "lit", ["k"] = l.Value.Kind.ToString() };
                lit["v"] = l.Value.Kind switch
                {
                    RuleValueKind.Number => JsonValue.Create(l.Value.AsNumber()),
                    RuleValueKind.Boolean => JsonValue.Create(l.Value.AsBool()),
                    RuleValueKind.Text => JsonValue.Create(l.Value.AsText()),
                    _ => null,
                };
                return lit;
            case VariableExpr v:
                return new JsonObject { ["t"] = "var", ["n"] = v.Name };
            case UnaryExpr u:
                return new JsonObject { ["t"] = "un", ["op"] = u.Op.ToString(), ["x"] = ToNode(u.Operand) };
            case BinaryExpr b:
                return new JsonObject
                {
                    ["t"] = "bin",
                    ["op"] = b.Op.ToString(),
                    ["l"] = ToNode(b.Left),
                    ["r"] = ToNode(b.Right),
                };
            case FunctionCallExpr f:
                var args = new JsonArray();
                foreach (var a in f.Arguments) args.Add(ToNode(a));
                return new JsonObject { ["t"] = "fn", ["n"] = f.Name, ["a"] = args };
            default:
                throw new ExpressionException($"Cannot serialize node '{expr.GetType().Name}'.");
        }
    }

    private static Expr FromNode(JsonNode node)
    {
        var obj = node.AsObject();
        var type = (string?)obj["t"] ?? throw new ExpressionException("AST node missing 't'.");
        switch (type)
        {
            case "lit":
                var kind = Enum.Parse<RuleValueKind>((string?)obj["k"] ?? "Null");
                var v = obj["v"];
                return new LiteralExpr(kind switch
                {
                    RuleValueKind.Number => RuleValue.Number((decimal)v!.GetValue<double>()),
                    RuleValueKind.Boolean => RuleValue.Bool(v!.GetValue<bool>()),
                    RuleValueKind.Text => RuleValue.Text(v!.GetValue<string>()),
                    _ => RuleValue.Null,
                });
            case "var":
                return new VariableExpr((string?)obj["n"] ?? throw new ExpressionException("var missing 'n'."));
            case "un":
                return new UnaryExpr(
                    Enum.Parse<UnaryOp>((string?)obj["op"] ?? throw new ExpressionException("un missing 'op'.")),
                    FromNode(obj["x"] ?? throw new ExpressionException("un missing 'x'.")));
            case "bin":
                return new BinaryExpr(
                    Enum.Parse<BinaryOp>((string?)obj["op"] ?? throw new ExpressionException("bin missing 'op'.")),
                    FromNode(obj["l"] ?? throw new ExpressionException("bin missing 'l'.")),
                    FromNode(obj["r"] ?? throw new ExpressionException("bin missing 'r'.")));
            case "fn":
                var argsNode = obj["a"]?.AsArray() ?? new JsonArray();
                var args = new List<Expr>();
                foreach (var a in argsNode) args.Add(FromNode(a!));
                return new FunctionCallExpr((string?)obj["n"] ?? throw new ExpressionException("fn missing 'n'."), args);
            default:
                throw new ExpressionException($"Unknown AST node type '{type}'.");
        }
    }
}

/// <summary>Collects the distinct variable names an expression reads. Used to build the rule dependency
/// graph (a rule depends on whatever produces the variables it references).</summary>
public static class VariableExtractor
{
    public static IReadOnlySet<string> Extract(Expr expr)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        Walk(expr, set);
        return set;
    }

    private static void Walk(Expr expr, HashSet<string> acc)
    {
        switch (expr)
        {
            case VariableExpr v: acc.Add(v.Name); break;
            case UnaryExpr u: Walk(u.Operand, acc); break;
            case BinaryExpr b: Walk(b.Left, acc); Walk(b.Right, acc); break;
            case FunctionCallExpr f: foreach (var a in f.Arguments) Walk(a, acc); break;
        }
    }
}
