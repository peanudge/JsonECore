using System.Text.Json;
using JsonECore.Context;
using static JsonECore.JsonElementHelper;

namespace JsonECore.Operators;

/// <summary>
/// Implements the $mergeDeep operator.
/// </summary>
public class MergeDeepOperator : IOperator
{
    public string Name => "$mergeDeep";

    public JsonElement Execute(JsonElement template, EvaluationContext context, Func<JsonElement, EvaluationContext, JsonElement> render)
    {
        if (!template.TryGetProperty("$mergeDeep", out var source))
        {
            throw new JsonEException(JsonEErrorCodes.InvalidTemplate, "$mergeDeep requires source array", Name);
        }

        var sourceValue = render(source, context);

        if (sourceValue.ValueKind != JsonValueKind.Array)
        {
            throw new JsonEException(JsonEErrorCodes.TypeMismatch, "$mergeDeep source must be an array of objects", "array", GetTypeName(sourceValue));
        }

        JsonElement? result = null;

        foreach (var item in sourceValue.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
            {
                throw new JsonEException(JsonEErrorCodes.TypeMismatch, "$mergeDeep array items must be objects", "object", GetTypeName(item));
            }

            if (result == null)
            {
                result = item.Clone();
            }
            else
            {
                result = MergeDeep(result.Value, item);
            }
        }

        return result ?? CreateEmptyObject();
    }

    private JsonElement MergeDeep(JsonElement target, JsonElement source)
    {
        if (target.ValueKind != JsonValueKind.Object || source.ValueKind != JsonValueKind.Object)
        {
            return source.Clone();
        }

        var result = new Dictionary<string, JsonElement>();

        // Copy all properties from target
        foreach (var prop in target.EnumerateObject())
        {
            result[prop.Name] = prop.Value.Clone();
        }

        // Merge properties from source
        foreach (var prop in source.EnumerateObject())
        {
            if (result.TryGetValue(prop.Name, out var existing) &&
                existing.ValueKind == JsonValueKind.Object &&
                prop.Value.ValueKind == JsonValueKind.Object)
            {
                result[prop.Name] = MergeDeep(existing, prop.Value);
            }
            else
            {
                result[prop.Name] = prop.Value.Clone();
            }
        }

        return CreateObject(result);
    }
}
