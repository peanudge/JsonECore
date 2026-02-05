using System.Text.Json;
using System.Text.RegularExpressions;
using JsonECore.Context;
using static JsonECore.JsonElementHelper;

namespace JsonECore.Operators;

/// <summary>
/// Implements the $map operator.
/// </summary>
public class MapOperator : IOperator
{
    public string Name => "$map";

    private static readonly Regex EachRegex = new(@"^each\((\w+)(?:,\s*(\w+))?\)$", RegexOptions.Compiled);

    public JsonElement Execute(JsonElement template, EvaluationContext context, Func<JsonElement, EvaluationContext, JsonElement> render)
    {
        if (!template.TryGetProperty("$map", out var source))
        {
            throw new JsonEException(JsonEErrorCodes.InvalidTemplate, "$map requires source array or object", Name);
        }

        // Find the each(x) or each(x, i) property
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
                    itemVar = match.Groups[1].Value;
                    if (match.Groups[2].Success)
                    {
                        indexVar = match.Groups[2].Value;
                    }
                    bodyTemplate = prop.Value;
                    break;
                }
            }
        }

        if (itemVar == null || bodyTemplate == null)
        {
            throw new JsonEException(JsonEErrorCodes.InvalidTemplate, "$map requires 'each(var)' or 'each(var, index)' property", Name);
        }

        var sourceValue = render(source, context);

        if (sourceValue.ValueKind == JsonValueKind.Array)
        {
            return MapArray(sourceValue, itemVar, indexVar, bodyTemplate.Value, context, render);
        }
        else if (sourceValue.ValueKind == JsonValueKind.Object)
        {
            return MapObject(sourceValue, itemVar, indexVar, bodyTemplate.Value, context, render);
        }

        throw new JsonEException(JsonEErrorCodes.TypeMismatch, "$map source must be an array or object", "array/object", GetTypeName(sourceValue));
    }

    private JsonElement MapArray(JsonElement array, string itemVar, string? indexVar, JsonElement body, EvaluationContext context, Func<JsonElement, EvaluationContext, JsonElement> render)
    {
        var results = new List<JsonElement>();
        int index = 0;

        foreach (var item in array.EnumerateArray())
        {
            var vars = new Dictionary<string, JsonElement> { { itemVar, item.Clone() } };
            if (indexVar != null)
            {
                vars[indexVar] = CreateNumber(index);
            }

            var childContext = context.CreateChildContext(vars);
            var result = render(body, childContext);

            // Skip if result is marked for deletion (null from $if without else)
            if (result.ValueKind != JsonValueKind.Undefined)
            {
                results.Add(result);
            }
            index++;
        }

        return CreateArray(results);
    }

    private JsonElement MapObject(JsonElement obj, string itemVar, string? indexVar, JsonElement body, EvaluationContext context, Func<JsonElement, EvaluationContext, JsonElement> render)
    {
        var results = new List<JsonElement>();

        foreach (var prop in obj.EnumerateObject())
        {
            // Create object with key and val
            var entryJson = JsonSerializer.Serialize(new { key = prop.Name, val = prop.Value });
            using var entryDoc = JsonDocument.Parse(entryJson);
            var entry = entryDoc.RootElement.Clone();

            var vars = new Dictionary<string, JsonElement> { { itemVar, entry } };
            if (indexVar != null)
            {
                vars[indexVar] = CreateString(prop.Name);
            }

            var childContext = context.CreateChildContext(vars);
            var result = render(body, childContext);

            if (result.ValueKind != JsonValueKind.Undefined)
            {
                results.Add(result);
            }
        }

        return CreateArray(results);
    }
}
