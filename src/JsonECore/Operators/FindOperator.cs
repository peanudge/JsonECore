using System.Text.Json;
using System.Text.RegularExpressions;
using JsonECore.Context;
using JsonECore.Expressions;

namespace JsonECore.Operators;

/// <summary>
/// Implements the $find operator.
/// </summary>
public class FindOperator : IOperator
{
    public string Name => "$find";

    private static readonly Regex EachRegex = new(@"^each\((\w+)\)$", RegexOptions.Compiled);

    public JsonElement Execute(JsonElement template, EvaluationContext context, Func<JsonElement, EvaluationContext, JsonElement> render)
    {
        if (!template.TryGetProperty("$find", out var source))
        {
            throw new JsonEException(JsonEErrorCodes.InvalidTemplate, "$find requires source array", Name);
        }

        // Find the each(x) property
        string? itemVar = null;
        JsonElement? conditionTemplate = null;

        foreach (var prop in template.EnumerateObject())
        {
            if (prop.Name.StartsWith("each("))
            {
                var match = EachRegex.Match(prop.Name);
                if (match.Success)
                {
                    itemVar = match.Groups[1].Value;
                    conditionTemplate = prop.Value;
                    break;
                }
            }
        }

        if (itemVar == null || conditionTemplate == null)
        {
            throw new JsonEException(JsonEErrorCodes.InvalidTemplate, "$find requires 'each(var)' property with condition", Name);
        }

        var sourceValue = render(source, context);

        if (sourceValue.ValueKind != JsonValueKind.Array)
        {
            throw new JsonEException(JsonEErrorCodes.TypeMismatch, "$find source must be an array", "array", GetTypeName(sourceValue));
        }

        foreach (var item in sourceValue.EnumerateArray())
        {
            var childContext = context.CreateChildContext(itemVar, item);

            bool matches;
            if (conditionTemplate.Value.ValueKind == JsonValueKind.String)
            {
                var result = ExpressionEvaluator.Evaluate(conditionTemplate.Value.GetString()!, childContext);
                matches = ExpressionEvaluator.IsTruthy(result);
            }
            else
            {
                var result = render(conditionTemplate.Value, childContext);
                matches = ExpressionEvaluator.IsTruthy(result);
            }

            if (matches)
            {
                return item.Clone();
            }
        }

        // Return null if not found
        using var doc = JsonDocument.Parse("null");
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
