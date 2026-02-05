using System.Text.Json;
using JsonECore.Context;
using static JsonECore.JsonElementHelper;

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

        if (!IsObject(obj))
            throw new JsonEException(JsonEErrorCodes.TypeMismatch, $"Cannot access property '{PropertyName}' on non-object", "object", GetTypeName(obj));

        if (obj.TryGetProperty(PropertyName, out var value))
            return value.Clone();

        throw new JsonEException(JsonEErrorCodes.UndefinedVariable, $"Property '{PropertyName}' not found", PropertyName);
    }
}
