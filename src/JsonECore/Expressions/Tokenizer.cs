namespace JsonECore.Expressions;

/// <summary>
/// Token types for the expression lexer.
/// </summary>
public enum TokenType
{
    // Literals
    Number,
    String,
    True,
    False,
    Null,

    // Identifiers
    Identifier,

    // Operators
    Plus,           // +
    Minus,          // -
    Star,           // *
    Slash,          // /
    Percent,        // %
    DoubleStar,     // **

    // Comparison
    Equal,          // ==
    NotEqual,       // !=
    Less,           // <
    LessEqual,      // <=
    Greater,        // >
    GreaterEqual,   // >=

    // Logical
    And,            // &&
    Or,             // ||
    Not,            // !

    // Keywords
    In,             // in

    // Punctuation
    LeftParen,      // (
    RightParen,     // )
    LeftBracket,    // [
    RightBracket,   // ]
    LeftBrace,      // {
    RightBrace,     // }
    Comma,          // ,
    Dot,            // .
    Colon,          // :
    Question,       // ?

    // Special
    EOF,
    Invalid
}

/// <summary>
/// Represents a token from the lexer.
/// </summary>
public class Token
{
    public TokenType Type { get; }
    public string Value { get; }
    public int Position { get; }

    public Token(TokenType type, string value, int position)
    {
        Type = type;
        Value = value;
        Position = position;
    }

    public override string ToString() => $"{Type}({Value}) at {Position}";
}

/// <summary>
/// Tokenizer for JSON-E expressions.
/// </summary>
public class Tokenizer
{
    private readonly string _input;
    private int _position;

    public Tokenizer(string input)
    {
        _input = input;
        _position = 0;
    }

    public List<Token> Tokenize()
    {
        var tokens = new List<Token>();

        while (!IsAtEnd())
        {
            SkipWhitespace();
            if (IsAtEnd()) break;

            var token = NextToken();
            if (token.Type != TokenType.Invalid)
            {
                tokens.Add(token);
            }
        }

        tokens.Add(new Token(TokenType.EOF, "", _position));
        return tokens;
    }

    private Token NextToken()
    {
        var start = _position;
        var ch = Current();

        // Numbers
        if (char.IsDigit(ch) || (ch == '.' && _position + 1 < _input.Length && char.IsDigit(_input[_position + 1])))
        {
            return ReadNumber();
        }

        // Strings
        if (ch == '"' || ch == '\'')
        {
            return ReadString(ch);
        }

        // Identifiers and keywords
        if (char.IsLetter(ch) || ch == '_')
        {
            return ReadIdentifier();
        }

        // Operators and punctuation
        switch (ch)
        {
            case '+':
                Advance();
                return new Token(TokenType.Plus, "+", start);
            case '-':
                Advance();
                return new Token(TokenType.Minus, "-", start);
            case '*':
                Advance();
                if (Match('*'))
                {
                    return new Token(TokenType.DoubleStar, "**", start);
                }
                return new Token(TokenType.Star, "*", start);
            case '/':
                Advance();
                return new Token(TokenType.Slash, "/", start);
            case '%':
                Advance();
                return new Token(TokenType.Percent, "%", start);
            case '=':
                Advance();
                if (Match('='))
                {
                    return new Token(TokenType.Equal, "==", start);
                }
                throw new JsonEException(JsonEErrorCodes.SyntaxError, $"Unexpected character '=' at position {start}", start, "=");
            case '!':
                Advance();
                if (Match('='))
                {
                    return new Token(TokenType.NotEqual, "!=", start);
                }
                return new Token(TokenType.Not, "!", start);
            case '<':
                Advance();
                if (Match('='))
                {
                    return new Token(TokenType.LessEqual, "<=", start);
                }
                return new Token(TokenType.Less, "<", start);
            case '>':
                Advance();
                if (Match('='))
                {
                    return new Token(TokenType.GreaterEqual, ">=", start);
                }
                return new Token(TokenType.Greater, ">", start);
            case '&':
                Advance();
                if (Match('&'))
                {
                    return new Token(TokenType.And, "&&", start);
                }
                throw new JsonEException(JsonEErrorCodes.SyntaxError, $"Expected '&&' at position {start}", start, "&");
            case '|':
                Advance();
                if (Match('|'))
                {
                    return new Token(TokenType.Or, "||", start);
                }
                throw new JsonEException(JsonEErrorCodes.SyntaxError, $"Expected '||' at position {start}", start, "|");
            case '(':
                Advance();
                return new Token(TokenType.LeftParen, "(", start);
            case ')':
                Advance();
                return new Token(TokenType.RightParen, ")", start);
            case '[':
                Advance();
                return new Token(TokenType.LeftBracket, "[", start);
            case ']':
                Advance();
                return new Token(TokenType.RightBracket, "]", start);
            case '{':
                Advance();
                return new Token(TokenType.LeftBrace, "{", start);
            case '}':
                Advance();
                return new Token(TokenType.RightBrace, "}", start);
            case ',':
                Advance();
                return new Token(TokenType.Comma, ",", start);
            case '.':
                Advance();
                return new Token(TokenType.Dot, ".", start);
            case ':':
                Advance();
                return new Token(TokenType.Colon, ":", start);
            case '?':
                Advance();
                return new Token(TokenType.Question, "?", start);
            default:
                Advance();
                throw new JsonEException(JsonEErrorCodes.SyntaxError, $"Unexpected character '{ch}' at position {start}", start, ch.ToString());
        }
    }

    private Token ReadNumber()
    {
        var start = _position;
        var sb = new System.Text.StringBuilder();

        // Integer part
        while (!IsAtEnd() && char.IsDigit(Current()))
        {
            sb.Append(Advance());
        }

        // Decimal part
        if (!IsAtEnd() && Current() == '.' && _position + 1 < _input.Length && char.IsDigit(_input[_position + 1]))
        {
            sb.Append(Advance()); // '.'
            while (!IsAtEnd() && char.IsDigit(Current()))
            {
                sb.Append(Advance());
            }
        }

        // Exponent part
        if (!IsAtEnd() && (Current() == 'e' || Current() == 'E'))
        {
            sb.Append(Advance());
            if (!IsAtEnd() && (Current() == '+' || Current() == '-'))
            {
                sb.Append(Advance());
            }
            while (!IsAtEnd() && char.IsDigit(Current()))
            {
                sb.Append(Advance());
            }
        }

        return new Token(TokenType.Number, sb.ToString(), start);
    }

    private Token ReadString(char quote)
    {
        var start = _position;
        Advance(); // Opening quote
        var sb = new System.Text.StringBuilder();

        while (!IsAtEnd() && Current() != quote)
        {
            if (Current() == '\\')
            {
                Advance();
                if (IsAtEnd())
                {
                    throw new JsonEException(JsonEErrorCodes.SyntaxError, "Unterminated string", start, "");
                }

                sb.Append(Current() switch
                {
                    'n' => '\n',
                    't' => '\t',
                    'r' => '\r',
                    '\\' => '\\',
                    '\'' => '\'',
                    '"' => '"',
                    _ => Current()
                });
                Advance();
            }
            else
            {
                sb.Append(Advance());
            }
        }

        if (IsAtEnd())
        {
            throw new JsonEException(JsonEErrorCodes.SyntaxError, "Unterminated string", start, "");
        }

        Advance(); // Closing quote
        return new Token(TokenType.String, sb.ToString(), start);
    }

    private Token ReadIdentifier()
    {
        var start = _position;
        var sb = new System.Text.StringBuilder();

        while (!IsAtEnd() && (char.IsLetterOrDigit(Current()) || Current() == '_'))
        {
            sb.Append(Advance());
        }

        var identifier = sb.ToString();
        var type = identifier switch
        {
            "true" => TokenType.True,
            "false" => TokenType.False,
            "null" => TokenType.Null,
            "in" => TokenType.In,
            _ => TokenType.Identifier
        };

        return new Token(type, identifier, start);
    }

    private void SkipWhitespace()
    {
        while (!IsAtEnd() && char.IsWhiteSpace(Current()))
        {
            Advance();
        }
    }

    private char Current() => _input[_position];

    private char Advance()
    {
        return _input[_position++];
    }

    private bool Match(char expected)
    {
        if (IsAtEnd() || Current() != expected)
        {
            return false;
        }
        _position++;
        return true;
    }

    private bool IsAtEnd() => _position >= _input.Length;
}
