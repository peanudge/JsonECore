using System.Text.Json;
using JsonECore.Context;

namespace JsonECore.Operators;

/// <summary>
/// Implements the $reverse operator.
/// </summary>
public class ReverseOperator : IOperator
{
    public string Name => "$reverse";

    public JsonElement Execute(JsonElement template, EvaluationContext context, Func<JsonElement, EvaluationContext, JsonElement> render)
    {
        if (!template.TryGetProperty("$reverse", out var source))
        {
            throw new JsonEException(JsonEErrorCodes.InvalidTemplate, "$reverse requires source array or string", Name);
        }

        var sourceValue = render(source, context);

        if (sourceValue.ValueKind == JsonValueKind.Array)
        {
            var items = sourceValue.EnumerateArray().Select(x => x.Clone()).ToList();
            items.Reverse();
            return CreateArray(items);
        }

        if (sourceValue.ValueKind == JsonValueKind.String)
        {
            var str = sourceValue.GetString()!;
            var chars = str.ToCharArray();
            Array.Reverse(chars);
            return CreateString(new string(chars));
        }

        throw new JsonEException(JsonEErrorCodes.TypeMismatch, "$reverse source must be an array or string", "array/string", GetTypeName(sourceValue));
    }

    private static JsonElement CreateArray(List<JsonElement> items)
    {
        var json = JsonSerializer.Serialize(items);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    private static JsonElement CreateString(string value)
    {
        var json = JsonSerializer.Serialize(value);
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
