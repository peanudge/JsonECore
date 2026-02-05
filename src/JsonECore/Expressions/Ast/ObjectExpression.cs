using System.Text.Json;
using JsonECore.Context;
using static JsonECore.JsonElementHelper;

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
            if (!IsString(key))
                throw new JsonEException(JsonEErrorCodes.TypeMismatch, "Object key must be a string", "string", GetTypeName(key));
            result[key.GetString()!] = valueExpr.Evaluate(context).Clone();
        }
        return CreateObject(result);
    }
}
