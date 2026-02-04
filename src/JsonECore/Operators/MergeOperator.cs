using System.Text.Json;
using JsonECore.Context;

namespace JsonECore.Operators;

/// <summary>
/// Implements the $merge operator.
/// </summary>
public class MergeOperator : IOperator
{
    public string Name => "$merge";

    public JsonElement Execute(JsonElement template, EvaluationContext context, Func<JsonElement, EvaluationContext, JsonElement> render)
    {
        if (!template.TryGetProperty("$merge", out var source))
        {
            throw new JsonEException(JsonEErrorCodes.InvalidTemplate, "$merge requires source array", Name);
        }

        var sourceValue = render(source, context);

        if (sourceValue.ValueKind != JsonValueKind.Array)
        {
            throw new JsonEException(JsonEErrorCodes.TypeMismatch, "$merge source must be an array of objects", "array", GetTypeName(sourceValue));
        }

        var result = new Dictionary<string, JsonElement>();

        foreach (var item in sourceValue.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
            {
                throw new JsonEException(JsonEErrorCodes.TypeMismatch, "$merge array items must be objects", "object", GetTypeName(item));
            }

            foreach (var prop in item.EnumerateObject())
            {
                result[prop.Name] = prop.Value.Clone();
            }
        }

        return CreateObject(result);
    }

    private static JsonElement CreateObject(Dictionary<string, JsonElement> properties)
    {
        var json = JsonSerializer.Serialize(properties);
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
