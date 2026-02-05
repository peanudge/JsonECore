using System.Text.Json;
using JsonECore.Context;
using static JsonECore.JsonElementHelper;

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
}
