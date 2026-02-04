using System.Text.Json;
using JsonECore.Context;

namespace JsonECore.Operators;

/// <summary>
/// Implements the $let operator.
/// </summary>
public class LetOperator : IOperator
{
    public string Name => "$let";

    public JsonElement Execute(JsonElement template, EvaluationContext context, Func<JsonElement, EvaluationContext, JsonElement> render)
    {
        if (!template.TryGetProperty("$let", out var bindings))
        {
            throw new JsonEException(JsonEErrorCodes.InvalidTemplate, "$let requires bindings object", Name);
        }

        if (!template.TryGetProperty("in", out var body))
        {
            throw new JsonEException(JsonEErrorCodes.InvalidTemplate, "$let requires 'in' property", Name);
        }

        if (bindings.ValueKind != JsonValueKind.Object)
        {
            throw new JsonEException(JsonEErrorCodes.TypeMismatch, "$let bindings must be an object", "object", GetTypeName(bindings));
        }

        // Create child context with new bindings
        var vars = new Dictionary<string, JsonElement>();
        foreach (var prop in bindings.EnumerateObject())
        {
            vars[prop.Name] = render(prop.Value, context);
        }

        var childContext = context.CreateChildContext(vars);
        return render(body, childContext);
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
