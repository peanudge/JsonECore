using System.Text.Json;
using JsonECore.Context;
using JsonECore.Expressions;

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

    private static JsonElement CreateArray(List<JsonElement> items)
    {
        var json = JsonSerializer.Serialize(items);
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
