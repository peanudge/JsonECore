using System.Text.Json;
using JsonECore.Context;
using JsonECore.Expressions;

namespace JsonECore.Operators;

/// <summary>
/// Implements the $eval operator.
/// </summary>
public class EvalOperator : IOperator
{
    public string Name => "$eval";

    public JsonElement Execute(JsonElement template, EvaluationContext context, Func<JsonElement, EvaluationContext, JsonElement> render)
    {
        if (!template.TryGetProperty("$eval", out var expression))
        {
            throw new JsonEException(JsonEErrorCodes.InvalidTemplate, "$eval requires expression value", Name);
        }

        if (expression.ValueKind != JsonValueKind.String)
        {
            throw new JsonEException(JsonEErrorCodes.TypeMismatch, "$eval expression must be a string", "string", GetTypeName(expression));
        }

        var expr = expression.GetString()!;
        return ExpressionEvaluator.Evaluate(expr, context);
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
