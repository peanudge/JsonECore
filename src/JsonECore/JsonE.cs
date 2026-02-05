using System.Text.Json;
using JsonECore.Context;
using JsonECore.Expressions;
using JsonECore.Operators;

namespace JsonECore;

/// <summary>
/// Main entry point for JSON-E template rendering.
/// </summary>
public static class JsonE
{
    /// <summary>
    /// Renders a JSON-E template with the given context.
    /// </summary>
    /// <param name="template">The template to render</param>
    /// <param name="context">The context containing variables</param>
    /// <returns>The rendered result</returns>
    public static JsonElement Render(JsonElement template, JsonElement context)
    {
        var evalContext = new EvaluationContext(context);
        return RenderInternal(template, evalContext);
    }

    /// <summary>
    /// Renders a JSON-E template with the given context.
    /// </summary>
    /// <param name="template">The template JSON string</param>
    /// <param name="context">The context JSON string</param>
    /// <returns>The rendered result as a JSON string</returns>
    public static string Render(string template, string context)
    {
        using var templateDoc = JsonDocument.Parse(template);
        using var contextDoc = JsonDocument.Parse(context);
        var result = Render(templateDoc.RootElement, contextDoc.RootElement);
        return result.GetRawText();
    }

    /// <summary>
    /// Renders a JSON-E template with an empty context.
    /// </summary>
    public static JsonElement Render(JsonElement template)
    {
        using var contextDoc = JsonDocument.Parse("{}");
        return Render(template, contextDoc.RootElement);
    }

    /// <summary>
    /// Validates a JSON-E template without rendering.
    /// </summary>
    /// <param name="template">The template to validate</param>
    /// <returns>True if valid, throws JsonEException otherwise</returns>
    public static bool Validate(JsonElement template)
    {
        try
        {
            ValidateInternal(template);
            return true;
        }
        catch (JsonEException)
        {
            throw;
        }
    }

    internal static JsonElement RenderInternal(JsonElement template, EvaluationContext context)
    {
        return template.ValueKind switch
        {
            JsonValueKind.Object => RenderObject(template, context),
            JsonValueKind.Array => RenderArray(template, context),
            JsonValueKind.String => RenderString(template, context),
            _ => template.Clone()
        };
    }

    private static JsonElement RenderObject(JsonElement template, EvaluationContext context)
    {
        // Check for operators
        var operatorKey = OperatorRegistry.Instance.FindOperatorKey(template);
        if (operatorKey != null)
        {
            if (OperatorRegistry.Instance.TryGetOperator(operatorKey, out var op) && op != null)
            {
                return op.Execute(template, context, RenderInternal);
            }
        }

        // Check for $-escaped keys
        var result = new Dictionary<string, JsonElement>();

        foreach (var prop in template.EnumerateObject())
        {
            var key = prop.Name;
            var value = prop.Value;

            // Handle escaped keys ($$key -> $key)
            if (key.StartsWith("$$"))
            {
                key = key.Substring(1);
            }
            // Handle expression keys ${expr}
            else if (key.StartsWith("$") && key.Contains("{") && key.EndsWith("}"))
            {
                // Dynamic key evaluation
                var expr = key.Substring(2, key.Length - 3); // Remove ${ and }
                var keyResult = ExpressionEvaluator.Evaluate(expr, context);
                if (keyResult.ValueKind != JsonValueKind.String)
                {
                    throw new JsonEException(JsonEErrorCodes.TypeMismatch, "Dynamic key must evaluate to string", "string", GetTypeName(keyResult));
                }
                key = keyResult.GetString()!;
            }

            // Check for conditional keys with $if suffix
            if (key.EndsWith(" $if"))
            {
                var actualKey = key.Substring(0, key.Length - 4);
                if (value.ValueKind != JsonValueKind.Object || !value.TryGetProperty("$if", out var condition))
                {
                    throw new JsonEException(JsonEErrorCodes.InvalidTemplate, "Key with $if suffix must have an object value with $if property", key);
                }

                bool conditionResult;
                if (condition.ValueKind == JsonValueKind.String)
                {
                    var condResult = ExpressionEvaluator.Evaluate(condition.GetString()!, context);
                    conditionResult = ExpressionEvaluator.IsTruthy(condResult);
                }
                else
                {
                    conditionResult = ExpressionEvaluator.IsTruthy(condition);
                }

                if (conditionResult && value.TryGetProperty("then", out var thenValue))
                {
                    result[actualKey] = RenderInternal(thenValue, context);
                }
                else if (!conditionResult && value.TryGetProperty("else", out var elseValue))
                {
                    result[actualKey] = RenderInternal(elseValue, context);
                }
                continue;
            }

            var renderedValue = RenderInternal(value, context);

            // Skip if the value should be deleted (from $if without else)
            if (IsDeleteMarker(renderedValue))
            {
                continue;
            }

            result[key] = renderedValue;
        }

        return CreateObject(result);
    }

    private static JsonElement RenderArray(JsonElement template, EvaluationContext context)
    {
        var result = new List<JsonElement>();

        foreach (var item in template.EnumerateArray())
        {
            var renderedItem = RenderInternal(item, context);

            // Skip if the value should be deleted
            if (!IsDeleteMarker(renderedItem))
            {
                result.Add(renderedItem);
            }
        }

        return CreateArray(result);
    }

    private static JsonElement RenderString(JsonElement template, EvaluationContext context)
    {
        var str = template.GetString()!;

        // Check for interpolation
        if (ExpressionEvaluator.HasInterpolation(str))
        {
            var result = ExpressionEvaluator.EvaluateInterpolation(str, context);
            return CreateString(result);
        }

        return template.Clone();
    }

    private static void ValidateInternal(JsonElement template)
    {
        switch (template.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var prop in template.EnumerateObject())
                {
                    if (prop.Name.StartsWith("$") && !prop.Name.StartsWith("$$"))
                    {
                        var opName = prop.Name;
                        // Check for valid operator or special keys
                        if (!OperatorRegistry.Instance.IsOperator(opName) &&
                            opName != "$default" &&
                            !opName.Contains("{") &&
                            !opName.EndsWith(" $if"))
                        {
                            throw new JsonEException(JsonEErrorCodes.InvalidOperator, $"Unknown operator: {opName}", opName);
                        }
                    }
                    ValidateInternal(prop.Value);
                }
                break;

            case JsonValueKind.Array:
                foreach (var item in template.EnumerateArray())
                {
                    ValidateInternal(item);
                }
                break;
        }
    }

    private static bool IsDeleteMarker(JsonElement value)
    {
        return DeleteMarker.IsDeleteMarker(value);
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

    private static JsonElement CreateObject(Dictionary<string, JsonElement> properties)
    {
        var json = JsonSerializer.Serialize(properties);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
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
}
