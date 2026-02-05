using System.Text.Json;
using JsonECore.Context;
using static JsonECore.JsonElementHelper;

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
}
