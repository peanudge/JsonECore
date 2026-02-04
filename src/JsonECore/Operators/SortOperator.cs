using System.Text.Json;
using System.Text.RegularExpressions;
using JsonECore.Context;
using JsonECore.Expressions;

namespace JsonECore.Operators;

/// <summary>
/// Implements the $sort operator.
/// </summary>
public class SortOperator : IOperator
{
    public string Name => "$sort";

    private static readonly Regex ByRegex = new(@"^by\((\w+)\)$", RegexOptions.Compiled);

    public JsonElement Execute(JsonElement template, EvaluationContext context, Func<JsonElement, EvaluationContext, JsonElement> render)
    {
        if (!template.TryGetProperty("$sort", out var source))
        {
            throw new JsonEException(JsonEErrorCodes.InvalidTemplate, "$sort requires source array", Name);
        }

        var sourceValue = render(source, context);

        if (sourceValue.ValueKind != JsonValueKind.Array)
        {
            throw new JsonEException(JsonEErrorCodes.TypeMismatch, "$sort source must be an array", "array", GetTypeName(sourceValue));
        }

        // Find the by(x) property if it exists
        string? itemVar = null;
        JsonElement? keyTemplate = null;

        foreach (var prop in template.EnumerateObject())
        {
            if (prop.Name.StartsWith("by("))
            {
                var match = ByRegex.Match(prop.Name);
                if (match.Success)
                {
                    itemVar = match.Groups[1].Value;
                    keyTemplate = prop.Value;
                    break;
                }
            }
        }

        var items = sourceValue.EnumerateArray().Select(x => x.Clone()).ToList();

        if (itemVar != null && keyTemplate.HasValue)
        {
            // Sort by key function
            items = items.OrderBy(item =>
            {
                var childContext = context.CreateChildContext(itemVar, item);
                JsonElement key;
                if (keyTemplate.Value.ValueKind == JsonValueKind.String)
                {
                    key = ExpressionEvaluator.Evaluate(keyTemplate.Value.GetString()!, childContext);
                }
                else
                {
                    key = render(keyTemplate.Value, childContext);
                }
                return GetSortKey(key);
            }).ToList();
        }
        else
        {
            // Sort by natural order
            items = items.OrderBy(GetSortKey).ToList();
        }

        return CreateArray(items);
    }

    private static object GetSortKey(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.Number => value.GetDouble(),
            JsonValueKind.String => value.GetString()!,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => (object)null!,
            _ => value.GetRawText()
        };
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
