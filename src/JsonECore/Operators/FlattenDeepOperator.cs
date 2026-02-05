using System.Text.Json;
using JsonECore.Context;
using static JsonECore.JsonElementHelper;

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
}
