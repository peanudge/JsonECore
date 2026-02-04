using System.Text.Json;
using JsonECore.Expressions.Ast;

namespace JsonECore.Expressions;

/// <summary>
/// Recursive descent parser for JSON-E expressions.
/// </summary>
public class ExpressionParser
{
    private readonly List<Token> _tokens;
    private int _position;

    public ExpressionParser(List<Token> tokens)
    {
        _tokens = tokens;
        _position = 0;
    }

    public static IExpression Parse(string expression)
    {
        var tokenizer = new Tokenizer(expression);
        var tokens = tokenizer.Tokenize();
        var parser = new ExpressionParser(tokens);
        return parser.ParseExpression();
    }

    public IExpression ParseExpression()
    {
        return ParseConditional();
    }

    private IExpression ParseConditional()
    {
        var expr = ParseOr();

        if (Match(TokenType.Question))
        {
            var trueExpr = ParseConditional();
            Consume(TokenType.Colon, "Expected ':' in conditional expression");
            var falseExpr = ParseConditional();
            return new ConditionalExpression(expr, trueExpr, falseExpr);
        }

        return expr;
    }

    private IExpression ParseOr()
    {
        var expr = ParseAnd();

        while (Match(TokenType.Or))
        {
            var right = ParseAnd();
            expr = new BinaryExpression(expr, "||", right);
        }

        return expr;
    }

    private IExpression ParseAnd()
    {
        var expr = ParseEquality();

        while (Match(TokenType.And))
        {
            var right = ParseEquality();
            expr = new BinaryExpression(expr, "&&", right);
        }

        return expr;
    }

    private IExpression ParseEquality()
    {
        var expr = ParseComparison();

        while (true)
        {
            if (Match(TokenType.Equal))
            {
                var right = ParseComparison();
                expr = new BinaryExpression(expr, "==", right);
            }
            else if (Match(TokenType.NotEqual))
            {
                var right = ParseComparison();
                expr = new BinaryExpression(expr, "!=", right);
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    private IExpression ParseComparison()
    {
        var expr = ParseIn();

        while (true)
        {
            if (Match(TokenType.Less))
            {
                var right = ParseIn();
                expr = new BinaryExpression(expr, "<", right);
            }
            else if (Match(TokenType.LessEqual))
            {
                var right = ParseIn();
                expr = new BinaryExpression(expr, "<=", right);
            }
            else if (Match(TokenType.Greater))
            {
                var right = ParseIn();
                expr = new BinaryExpression(expr, ">", right);
            }
            else if (Match(TokenType.GreaterEqual))
            {
                var right = ParseIn();
                expr = new BinaryExpression(expr, ">=", right);
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    private IExpression ParseIn()
    {
        var expr = ParseAdditive();

        while (Match(TokenType.In))
        {
            var right = ParseAdditive();
            expr = new BinaryExpression(expr, "in", right);
        }

        return expr;
    }

    private IExpression ParseAdditive()
    {
        var expr = ParseMultiplicative();

        while (true)
        {
            if (Match(TokenType.Plus))
            {
                var right = ParseMultiplicative();
                expr = new BinaryExpression(expr, "+", right);
            }
            else if (Match(TokenType.Minus))
            {
                var right = ParseMultiplicative();
                expr = new BinaryExpression(expr, "-", right);
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    private IExpression ParseMultiplicative()
    {
        var expr = ParsePower();

        while (true)
        {
            if (Match(TokenType.Star))
            {
                var right = ParsePower();
                expr = new BinaryExpression(expr, "*", right);
            }
            else if (Match(TokenType.Slash))
            {
                var right = ParsePower();
                expr = new BinaryExpression(expr, "/", right);
            }
            else if (Match(TokenType.Percent))
            {
                var right = ParsePower();
                expr = new BinaryExpression(expr, "%", right);
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    private IExpression ParsePower()
    {
        var expr = ParseUnary();

        if (Match(TokenType.DoubleStar))
        {
            var right = ParsePower(); // Right associative
            expr = new BinaryExpression(expr, "**", right);
        }

        return expr;
    }

    private IExpression ParseUnary()
    {
        if (Match(TokenType.Not))
        {
            var right = ParseUnary();
            return new UnaryExpression("!", right);
        }

        if (Match(TokenType.Minus))
        {
            var right = ParseUnary();
            return new UnaryExpression("-", right);
        }

        if (Match(TokenType.Plus))
        {
            var right = ParseUnary();
            return new UnaryExpression("+", right);
        }

        return ParsePostfix();
    }

    private IExpression ParsePostfix()
    {
        var expr = ParsePrimary();

        while (true)
        {
            if (Match(TokenType.Dot))
            {
                var name = Consume(TokenType.Identifier, "Expected property name after '.'");
                expr = new PropertyAccessExpression(expr, name.Value);
            }
            else if (Match(TokenType.LeftBracket))
            {
                // Check for slice
                IExpression? start = null;
                IExpression? end = null;

                if (!Check(TokenType.Colon))
                {
                    start = ParseExpression();
                }

                if (Match(TokenType.Colon))
                {
                    if (!Check(TokenType.RightBracket))
                    {
                        end = ParseExpression();
                    }
                    Consume(TokenType.RightBracket, "Expected ']' after slice");
                    expr = new SliceExpression(expr, start, end);
                }
                else
                {
                    Consume(TokenType.RightBracket, "Expected ']' after index");
                    expr = new IndexExpression(expr, start!);
                }
            }
            else if (Match(TokenType.LeftParen))
            {
                // Function call - expr must be an identifier
                if (expr is IdentifierExpression identExpr)
                {
                    var args = ParseArguments();
                    Consume(TokenType.RightParen, "Expected ')' after arguments");
                    expr = new CallExpression(identExpr.Name, args);
                }
                else
                {
                    throw new JsonEException(JsonEErrorCodes.SyntaxError, "Cannot call non-identifier", Previous().Position, "");
                }
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    private List<IExpression> ParseArguments()
    {
        var args = new List<IExpression>();

        if (!Check(TokenType.RightParen))
        {
            do
            {
                args.Add(ParseExpression());
            } while (Match(TokenType.Comma));
        }

        return args;
    }

    private IExpression ParsePrimary()
    {
        // Literals
        if (Match(TokenType.Number))
        {
            var value = double.Parse(Previous().Value, System.Globalization.CultureInfo.InvariantCulture);
            return LiteralExpression.FromValue(value);
        }

        if (Match(TokenType.String))
        {
            return LiteralExpression.FromValue(Previous().Value);
        }

        if (Match(TokenType.True))
        {
            return LiteralExpression.FromValue(true);
        }

        if (Match(TokenType.False))
        {
            return LiteralExpression.FromValue(false);
        }

        if (Match(TokenType.Null))
        {
            return LiteralExpression.FromValue(null);
        }

        // Identifier
        if (Match(TokenType.Identifier))
        {
            return new IdentifierExpression(Previous().Value);
        }

        // Parenthesized expression
        if (Match(TokenType.LeftParen))
        {
            var expr = ParseExpression();
            Consume(TokenType.RightParen, "Expected ')' after expression");
            return expr;
        }

        // Array literal
        if (Match(TokenType.LeftBracket))
        {
            var elements = new List<IExpression>();
            if (!Check(TokenType.RightBracket))
            {
                do
                {
                    elements.Add(ParseExpression());
                } while (Match(TokenType.Comma));
            }
            Consume(TokenType.RightBracket, "Expected ']' after array elements");
            return new ArrayExpression(elements);
        }

        // Object literal
        if (Match(TokenType.LeftBrace))
        {
            var properties = new List<(IExpression Key, IExpression Value)>();
            if (!Check(TokenType.RightBrace))
            {
                do
                {
                    IExpression key;
                    if (Match(TokenType.String))
                    {
                        key = LiteralExpression.FromValue(Previous().Value);
                    }
                    else if (Match(TokenType.Identifier))
                    {
                        key = LiteralExpression.FromValue(Previous().Value);
                    }
                    else
                    {
                        throw new JsonEException(JsonEErrorCodes.SyntaxError, "Expected property name", Current().Position, "");
                    }

                    Consume(TokenType.Colon, "Expected ':' after property name");
                    var value = ParseExpression();
                    properties.Add((key, value));
                } while (Match(TokenType.Comma));
            }
            Consume(TokenType.RightBrace, "Expected '}' after object properties");
            return new ObjectExpression(properties);
        }

        throw new JsonEException(JsonEErrorCodes.SyntaxError, $"Unexpected token '{Current().Value}'", Current().Position, Current().Value);
    }

    private Token Current() => _tokens[_position];
    private Token Previous() => _tokens[_position - 1];

    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Current().Type == type;
    }

    private bool Match(TokenType type)
    {
        if (Check(type))
        {
            _position++;
            return true;
        }
        return false;
    }

    private Token Consume(TokenType type, string message)
    {
        if (Check(type))
        {
            return _tokens[_position++];
        }
        throw new JsonEException(JsonEErrorCodes.SyntaxError, message, Current().Position, Current().Value);
    }

    private bool IsAtEnd() => Current().Type == TokenType.EOF;
}
