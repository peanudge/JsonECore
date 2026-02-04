using System.Text.Json;
using JsonECore.Context;
using JsonECore.Expressions;

namespace JsonECore.Operators;

/// <summary>
/// Implements the $switch operator.
/// </summary>
public class SwitchOperator : IOperator
{
    public string Name => "$switch";

    public JsonElement Execute(JsonElement template, EvaluationContext context, Func<JsonElement, EvaluationContext, JsonElement> render)
    {
        if (!template.TryGetProperty("$switch", out var cases))
        {
            throw new JsonEException(JsonEErrorCodes.InvalidTemplate, "$switch requires cases object", Name);
        }

        if (cases.ValueKind != JsonValueKind.Object)
        {
            throw new JsonEException(JsonEErrorCodes.TypeMismatch, "$switch cases must be an object", "object", GetTypeName(cases));
        }

        JsonElement? defaultValue = null;

        foreach (var prop in cases.EnumerateObject())
        {
            if (prop.Name == "$default")
            {
                defaultValue = prop.Value;
                continue;
            }

            var result = ExpressionEvaluator.Evaluate(prop.Name, context);
            if (ExpressionEvaluator.IsTruthy(result))
            {
                return render(prop.Value, context);
            }
        }

        if (defaultValue.HasValue)
        {
            return render(defaultValue.Value, context);
        }

        throw new JsonEException(JsonEErrorCodes.InvalidTemplate, "$switch has no matching case and no default", Name);
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
