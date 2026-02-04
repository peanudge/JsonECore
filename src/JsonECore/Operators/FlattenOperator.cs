using System.Text.Json;
using JsonECore.Context;

namespace JsonECore.Operators;

/// <summary>
/// Implements the $flatten operator.
/// </summary>
public class FlattenOperator : IOperator
{
    public string Name => "$flatten";

    public JsonElement Execute(JsonElement template, EvaluationContext context, Func<JsonElement, EvaluationContext, JsonElement> render)
    {
        if (!template.TryGetProperty("$flatten", out var source))
        {
            throw new JsonEException(JsonEErrorCodes.InvalidTemplate, "$flatten requires source array", Name);
        }

        var sourceValue = render(source, context);

        if (sourceValue.ValueKind != JsonValueKind.Array)
        {
            throw new JsonEException(JsonEErrorCodes.TypeMismatch, "$flatten source must be an array", "array", GetTypeName(sourceValue));
        }

        var results = new List<JsonElement>();

        foreach (var item in sourceValue.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.Array)
            {
                foreach (var subItem in item.EnumerateArray())
                {
                    results.Add(subItem.Clone());
                }
            }
            else
            {
                results.Add(item.Clone());
            }
        }

        return CreateArray(results);
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
