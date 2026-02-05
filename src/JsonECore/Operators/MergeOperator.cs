using System.Text.Json;
using JsonECore.Context;
using static JsonECore.JsonElementHelper;

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
}
