using System.Globalization;
using System.Text;

namespace HR.Domain.Engines.Finance.Expressions;

/// <summary>Parses a rule expression source string into an <see cref="Expr"/> AST.
///
/// Grammar (lowest → highest precedence):
///   or          := and ( "OR" and )*
///   and         := equality ( "AND" equality )*
///   equality    := comparison ( ("==" | "!=") comparison )*
///   comparison  := additive ( ("&lt;" | "&lt;=" | "&gt;" | "&gt;=") additive )*
///   additive    := multiplicative ( ("+" | "-") multiplicative )*
///   multiplicative := unary ( ("*" | "/" | "%") unary )*
///   unary       := ("NOT" | "!" | "-") unary | primary
///   primary     := NUMBER | STRING | "true" | "false" | "null"
///                | IDENT | IDENT "(" args ")" | "(" or ")"
///
/// Keywords (AND/OR/NOT/TRUE/FALSE/NULL) are case-insensitive; "&amp;&amp;", "||" and "!" are accepted as
/// synonyms. Identifiers may contain dots (e.g. Employee.Department). String literals use single or
/// double quotes.</summary>
public sealed class ExpressionParser
{
    private enum T
    {
        Number, String, Ident, True, False, Null,
        Plus, Minus, Star, Slash, Percent,
        EqEq, NotEq, Lt, LtEq, Gt, GtEq,
        And, Or, Not,
        LParen, RParen, Comma, Eof,
    }

    private readonly record struct Token(T Type, string Text, decimal Number);

    private readonly List<Token> _tokens;
    private int _pos;

    private ExpressionParser(List<Token> tokens)
    {
        _tokens = tokens;
    }

    /// <summary>Parse source text into an AST. Throws <see cref="ExpressionException"/> on any error.</summary>
    public static Expr Parse(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
            throw new ExpressionException("Expression is empty.");
        var tokens = Tokenize(source);
        var parser = new ExpressionParser(tokens);
        var expr = parser.ParseOr();
        parser.Expect(T.Eof, "end of expression");
        return expr;
    }

    /// <summary>Validate that source text parses; returns the error message, or null when valid.</summary>
    public static string? TryValidate(string source)
    {
        try { Parse(source); return null; }
        catch (ExpressionException ex) { return ex.Message; }
    }

    // ---- Tokenizer ----

    private static List<Token> Tokenize(string s)
    {
        var tokens = new List<Token>();
        var i = 0;
        while (i < s.Length)
        {
            var c = s[i];
            if (char.IsWhiteSpace(c)) { i++; continue; }

            if (char.IsDigit(c) || (c == '.' && i + 1 < s.Length && char.IsDigit(s[i + 1])))
            {
                var start = i;
                while (i < s.Length && (char.IsDigit(s[i]) || s[i] == '.')) i++;
                var text = s[start..i];
                if (!decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var num))
                    throw new ExpressionException($"Invalid number '{text}'.");
                tokens.Add(new Token(T.Number, text, num));
                continue;
            }

            if (c == '\'' || c == '"')
            {
                var quote = c;
                i++;
                var sb = new StringBuilder();
                while (i < s.Length && s[i] != quote)
                {
                    if (s[i] == '\\' && i + 1 < s.Length) { i++; sb.Append(s[i]); }
                    else sb.Append(s[i]);
                    i++;
                }
                if (i >= s.Length) throw new ExpressionException("Unterminated string literal.");
                i++; // closing quote
                tokens.Add(new Token(T.String, sb.ToString(), 0m));
                continue;
            }

            if (char.IsLetter(c) || c == '_')
            {
                var start = i;
                while (i < s.Length && (char.IsLetterOrDigit(s[i]) || s[i] == '_' || s[i] == '.')) i++;
                var word = s[start..i];
                tokens.Add(word.ToUpperInvariant() switch
                {
                    "AND" => new Token(T.And, word, 0m),
                    "OR" => new Token(T.Or, word, 0m),
                    "NOT" => new Token(T.Not, word, 0m),
                    "TRUE" => new Token(T.True, word, 0m),
                    "FALSE" => new Token(T.False, word, 0m),
                    "NULL" => new Token(T.Null, word, 0m),
                    _ => new Token(T.Ident, word, 0m),
                });
                continue;
            }

            // Operators / punctuation
            switch (c)
            {
                case '+': tokens.Add(new Token(T.Plus, "+", 0m)); i++; break;
                case '-': tokens.Add(new Token(T.Minus, "-", 0m)); i++; break;
                case '*': tokens.Add(new Token(T.Star, "*", 0m)); i++; break;
                case '/': tokens.Add(new Token(T.Slash, "/", 0m)); i++; break;
                case '%': tokens.Add(new Token(T.Percent, "%", 0m)); i++; break;
                case '(': tokens.Add(new Token(T.LParen, "(", 0m)); i++; break;
                case ')': tokens.Add(new Token(T.RParen, ")", 0m)); i++; break;
                case ',': tokens.Add(new Token(T.Comma, ",", 0m)); i++; break;
                case '=':
                    if (Next(s, i) == '=') { tokens.Add(new Token(T.EqEq, "==", 0m)); i += 2; }
                    else throw new ExpressionException("Use '==' for equality.");
                    break;
                case '!':
                    if (Next(s, i) == '=') { tokens.Add(new Token(T.NotEq, "!=", 0m)); i += 2; }
                    else { tokens.Add(new Token(T.Not, "!", 0m)); i++; }
                    break;
                case '<':
                    if (Next(s, i) == '=') { tokens.Add(new Token(T.LtEq, "<=", 0m)); i += 2; }
                    else { tokens.Add(new Token(T.Lt, "<", 0m)); i++; }
                    break;
                case '>':
                    if (Next(s, i) == '=') { tokens.Add(new Token(T.GtEq, ">=", 0m)); i += 2; }
                    else { tokens.Add(new Token(T.Gt, ">", 0m)); i++; }
                    break;
                case '&':
                    if (Next(s, i) == '&') { tokens.Add(new Token(T.And, "&&", 0m)); i += 2; }
                    else throw new ExpressionException("Use '&&' or 'AND'.");
                    break;
                case '|':
                    if (Next(s, i) == '|') { tokens.Add(new Token(T.Or, "||", 0m)); i += 2; }
                    else throw new ExpressionException("Use '||' or 'OR'.");
                    break;
                default:
                    throw new ExpressionException($"Unexpected character '{c}'.");
            }
        }
        tokens.Add(new Token(T.Eof, string.Empty, 0m));
        return tokens;
    }

    private static char Next(string s, int i) => i + 1 < s.Length ? s[i + 1] : '\0';

    // ---- Recursive-descent parser ----

    private Token Peek => _tokens[_pos];
    private Token Advance() => _tokens[_pos++];
    private bool Check(T type) => Peek.Type == type;

    private bool Match(params T[] types)
    {
        foreach (var t in types)
        {
            if (Check(t)) { _pos++; return true; }
        }
        return false;
    }

    private Token Expect(T type, string what)
    {
        if (!Check(type)) throw new ExpressionException($"Expected {what}.");
        return Advance();
    }

    private Expr ParseOr()
    {
        var expr = ParseAnd();
        while (Check(T.Or)) { Advance(); expr = new BinaryExpr(BinaryOp.Or, expr, ParseAnd()); }
        return expr;
    }

    private Expr ParseAnd()
    {
        var expr = ParseEquality();
        while (Check(T.And)) { Advance(); expr = new BinaryExpr(BinaryOp.And, expr, ParseEquality()); }
        return expr;
    }

    private Expr ParseEquality()
    {
        var expr = ParseComparison();
        while (Check(T.EqEq) || Check(T.NotEq))
        {
            var op = Advance().Type == T.EqEq ? BinaryOp.Equal : BinaryOp.NotEqual;
            expr = new BinaryExpr(op, expr, ParseComparison());
        }
        return expr;
    }

    private Expr ParseComparison()
    {
        var expr = ParseAdditive();
        while (Check(T.Lt) || Check(T.LtEq) || Check(T.Gt) || Check(T.GtEq))
        {
            var op = Advance().Type switch
            {
                T.Lt => BinaryOp.LessThan,
                T.LtEq => BinaryOp.LessThanOrEqual,
                T.Gt => BinaryOp.GreaterThan,
                _ => BinaryOp.GreaterThanOrEqual,
            };
            expr = new BinaryExpr(op, expr, ParseAdditive());
        }
        return expr;
    }

    private Expr ParseAdditive()
    {
        var expr = ParseMultiplicative();
        while (Check(T.Plus) || Check(T.Minus))
        {
            var op = Advance().Type == T.Plus ? BinaryOp.Add : BinaryOp.Subtract;
            expr = new BinaryExpr(op, expr, ParseMultiplicative());
        }
        return expr;
    }

    private Expr ParseMultiplicative()
    {
        var expr = ParseUnary();
        while (Check(T.Star) || Check(T.Slash) || Check(T.Percent))
        {
            var op = Advance().Type switch
            {
                T.Star => BinaryOp.Multiply,
                T.Slash => BinaryOp.Divide,
                _ => BinaryOp.Modulo,
            };
            expr = new BinaryExpr(op, expr, ParseUnary());
        }
        return expr;
    }

    private Expr ParseUnary()
    {
        if (Check(T.Not)) { Advance(); return new UnaryExpr(UnaryOp.Not, ParseUnary()); }
        if (Check(T.Minus)) { Advance(); return new UnaryExpr(UnaryOp.Negate, ParseUnary()); }
        return ParsePrimary();
    }

    private Expr ParsePrimary()
    {
        if (Check(T.Number)) return new LiteralExpr(RuleValue.Number(Advance().Number));
        if (Check(T.String)) return new LiteralExpr(RuleValue.Text(Advance().Text));
        if (Check(T.True)) { Advance(); return new LiteralExpr(RuleValue.Bool(true)); }
        if (Check(T.False)) { Advance(); return new LiteralExpr(RuleValue.Bool(false)); }
        if (Check(T.Null)) { Advance(); return new LiteralExpr(RuleValue.Null); }

        if (Check(T.LParen))
        {
            Advance();
            var inner = ParseOr();
            Expect(T.RParen, "')'");
            return inner;
        }

        if (Check(T.Ident))
        {
            var name = Advance().Text;
            if (Check(T.LParen))
            {
                Advance();
                var args = new List<Expr>();
                if (!Check(T.RParen))
                {
                    do { args.Add(ParseOr()); } while (Match(T.Comma));
                }
                Expect(T.RParen, "')' to close function arguments");
                return new FunctionCallExpr(name, args);
            }
            return new VariableExpr(name);
        }

        throw new ExpressionException($"Unexpected token '{Peek.Text}'.");
    }
}
