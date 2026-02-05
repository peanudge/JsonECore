using System.Text.Json;
using System.Text.RegularExpressions;
using JsonECore.Context;
using static JsonECore.JsonElementHelper;

namespace JsonECore.Operators;

/// <summary>
/// Implements the $reduce operator.
/// </summary>
public class ReduceOperator : IOperator
{
    public string Name => "$reduce";

    // Supports each(acc, val) or each(acc, val, index)
    private static readonly Regex EachRegex = new(@"^each\((\w+),\s*(\w+)(?:,\s*(\w+))?\)$", RegexOptions.Compiled);

    public JsonElement Execute(JsonElement template, EvaluationContext context, Func<JsonElement, EvaluationContext, JsonElement> render)
    {
        if (!template.TryGetProperty("$reduce", out var source))
        {
            throw new JsonEException(JsonEErrorCodes.InvalidTemplate, "$reduce requires source array", Name);
        }

        // Find the each(acc, x) or each(acc, x, index) property
        string? accVar = null;
        string? itemVar = null;
        string? indexVar = null;
        JsonElement? bodyTemplate = null;

        foreach (var prop in template.EnumerateObject())
        {
            if (prop.Name.StartsWith("each("))
            {
                var match = EachRegex.Match(prop.Name);
                if (match.Success)
                {
                    accVar = match.Groups[1].Value;
                    itemVar = match.Groups[2].Value;
                    if (match.Groups[3].Success)
                    {
                        indexVar = match.Groups[3].Value;
                    }
                    bodyTemplate = prop.Value;
                    break;
                }
            }
        }

        if (accVar == null || itemVar == null || bodyTemplate == null)
        {
            throw new JsonEException(JsonEErrorCodes.InvalidTemplate, "$reduce requires 'each(accumulator, value)' or 'each(accumulator, value, index)' property", Name);
        }

        if (!template.TryGetProperty("initial", out var initial))
        {
            throw new JsonEException(JsonEErrorCodes.InvalidTemplate, "$reduce requires 'initial' property", Name);
        }

        var sourceValue = render(source, context);

        if (sourceValue.ValueKind != JsonValueKind.Array)
        {
            throw new JsonEException(JsonEErrorCodes.TypeMismatch, "$reduce source must be an array", "array", GetTypeName(sourceValue));
        }

        var accumulator = render(initial, context);
        int index = 0;

        foreach (var item in sourceValue.EnumerateArray())
        {
            var vars = new Dictionary<string, JsonElement>
            {
                { accVar, accumulator.Clone() },
                { itemVar, item.Clone() }
            };

            if (indexVar != null)
            {
                vars[indexVar] = CreateNumber(index);
            }

            var childContext = context.CreateChildContext(vars);
            accumulator = render(bodyTemplate.Value, childContext);
            index++;
        }

        return accumulator;
    }
}
