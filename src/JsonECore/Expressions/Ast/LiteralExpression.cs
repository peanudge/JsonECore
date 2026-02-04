using System.Text.Json;
using JsonECore.Context;

namespace JsonECore.Expressions.Ast;

/// <summary>
/// Represents a literal value (number, string, boolean, null).
/// </summary>
public class LiteralExpression : IExpression
{
    private readonly JsonElement _value;

    public LiteralExpression(JsonElement value)
    {
        _value = value.Clone();
    }

    public static LiteralExpression FromValue(object? value)
    {
        var json = JsonSerializer.Serialize(value);
        using var doc = JsonDocument.Parse(json);
        return new LiteralExpression(doc.RootElement.Clone());
    }

    public JsonElement Evaluate(EvaluationContext context)
    {
        return _value;
    }
}
