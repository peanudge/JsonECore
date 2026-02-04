using System.Text.Json;
using JsonECore.Context;

namespace JsonECore.Expressions.Ast;

/// <summary>
/// Represents an object literal expression (e.g., {a: 1, b: 2}).
/// </summary>
public class ObjectExpression : IExpression
{
    public List<(IExpression Key, IExpression Value)> Properties { get; }

    public ObjectExpression(List<(IExpression Key, IExpression Value)> properties)
    {
        Properties = properties;
    }

    public JsonElement Evaluate(EvaluationContext context)
    {
        var result = new Dictionary<string, JsonElement>();
        foreach (var (keyExpr, valueExpr) in Properties)
        {
            var key = keyExpr.Evaluate(context);
            if (key.ValueKind != JsonValueKind.String)
            {
                throw new JsonEException(JsonEErrorCodes.TypeMismatch, "Object key must be a string", "string", GetTypeName(key));
            }
            var value = valueExpr.Evaluate(context);
            result[key.GetString()!] = value.Clone();
        }

        var json = JsonSerializer.Serialize(result);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    private static string GetTypeName(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.Null => "null",
            JsonValueKind.True => "boolean",
            JsonValueKind.False => "boolean",
            JsonValueKind.Number => "number",
            JsonValueKind.String => "string",
            JsonValueKind.Array => "array",
            JsonValueKind.Object => "object",
            _ => "undefined"
        };
    }
}
