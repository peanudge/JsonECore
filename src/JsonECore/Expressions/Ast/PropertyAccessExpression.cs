using System.Text.Json;
using JsonECore.Context;

namespace JsonECore.Expressions.Ast;

/// <summary>
/// Represents a property access (e.g., obj.property).
/// </summary>
public class PropertyAccessExpression : IExpression
{
    public IExpression Object { get; }
    public string PropertyName { get; }

    public PropertyAccessExpression(IExpression obj, string propertyName)
    {
        Object = obj;
        PropertyName = propertyName;
    }

    public JsonElement Evaluate(EvaluationContext context)
    {
        var obj = Object.Evaluate(context);

        if (obj.ValueKind != JsonValueKind.Object)
        {
            throw new JsonEException(JsonEErrorCodes.TypeMismatch, $"Cannot access property '{PropertyName}' on non-object", "object", GetTypeName(obj));
        }

        if (obj.TryGetProperty(PropertyName, out var value))
        {
            return value.Clone();
        }

        throw new JsonEException(JsonEErrorCodes.UndefinedVariable, $"Property '{PropertyName}' not found", PropertyName);
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
