using System.Text.Json;
using JsonECore.Context;

namespace JsonECore.Operators;

/// <summary>
/// Implements the $json operator.
/// </summary>
public class JsonOperator : IOperator
{
    public string Name => "$json";

    public JsonElement Execute(JsonElement template, EvaluationContext context, Func<JsonElement, EvaluationContext, JsonElement> render)
    {
        if (!template.TryGetProperty("$json", out var source))
        {
            throw new JsonEException(JsonEErrorCodes.InvalidTemplate, "$json requires source value", Name);
        }

        var sourceValue = render(source, context);
        var jsonString = sourceValue.GetRawText();

        return CreateString(jsonString);
    }

    private static JsonElement CreateString(string value)
    {
        var json = JsonSerializer.Serialize(value);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }
}
