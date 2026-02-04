using System.Text.Json;
using JsonECore.Context;

namespace JsonECore.Operators;

/// <summary>
/// Implements the $flattenDeep operator.
/// </summary>
public class FlattenDeepOperator : IOperator
{
    public string Name => "$flattenDeep";

    public JsonElement Execute(JsonElement template, EvaluationContext context, Func<JsonElement, EvaluationContext, JsonElement> render)
    {
        if (!template.TryGetProperty("$flattenDeep", out var source))
        {
            throw new JsonEException(JsonEErrorCodes.InvalidTemplate, "$flattenDeep requires source array", Name);
        }

        var sourceValue = render(source, context);

        if (sourceValue.ValueKind != JsonValueKind.Array)
        {
            throw new JsonEException(JsonEErrorCodes.TypeMismatch, "$flattenDeep source must be an array", "array", GetTypeName(sourceValue));
        }

        var results = new List<JsonElement>();
        FlattenRecursive(sourceValue, results);

        return CreateArray(results);
    }

    private void FlattenRecursive(JsonElement array, List<JsonElement> results)
    {
        foreach (var item in array.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.Array)
            {
                FlattenRecursive(item, results);
            }
            else
            {
                results.Add(item.Clone());
            }
        }
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
