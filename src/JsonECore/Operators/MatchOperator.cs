using System.Text.Json;
using JsonECore.Context;
using JsonECore.Expressions;
using static JsonECore.JsonElementHelper;

namespace JsonECore.Operators;

/// <summary>
/// Implements the $match operator.
/// </summary>
public class MatchOperator : IOperator
{
    public string Name => "$match";

    public JsonElement Execute(JsonElement template, EvaluationContext context, Func<JsonElement, EvaluationContext, JsonElement> render)
    {
        if (!template.TryGetProperty("$match", out var cases))
        {
            throw new JsonEException(JsonEErrorCodes.InvalidTemplate, "$match requires cases object", Name);
        }

        if (cases.ValueKind != JsonValueKind.Object)
        {
            throw new JsonEException(JsonEErrorCodes.TypeMismatch, "$match cases must be an object", "object", GetTypeName(cases));
        }

        var matches = new List<JsonElement>();

        foreach (var prop in cases.EnumerateObject())
        {
            var result = ExpressionEvaluator.Evaluate(prop.Name, context);
            if (ExpressionEvaluator.IsTruthy(result))
            {
                matches.Add(render(prop.Value, context));
            }
        }

        if (matches.Count == 0)
        {
            throw new JsonEException(JsonEErrorCodes.InvalidTemplate, "$match has no matching case", Name);
        }

        return CreateArray(matches);
    }
}
