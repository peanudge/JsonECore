using System.Text.Json;
using JsonECore.Context;

namespace JsonECore.Expressions.Ast;

/// <summary>
/// Represents a unary operation (e.g., !a, -a).
/// </summary>
public class UnaryExpression : IExpression
{
    public string Operator { get; }
    public IExpression Operand { get; }

    public UnaryExpression(string op, IExpression operand)
    {
        Operator = op;
        Operand = operand;
    }

    public JsonElement Evaluate(EvaluationContext context)
    {
        var value = Operand.Evaluate(context);

        return Operator switch
        {
            "!" => EvaluateNot(value),
            "-" => EvaluateNegate(value),
            "+" => EvaluatePositive(value),
            _ => throw new JsonEException(JsonEErrorCodes.SyntaxError, $"Unknown unary operator: {Operator}", 0, Operator)
        };
    }

    private static JsonElement EvaluateNot(JsonElement value)
    {
        var truthy = IsTruthy(value);
        return CreateBool(!truthy);
    }

    private static JsonElement EvaluateNegate(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Number)
        {
            throw new JsonEException(JsonEErrorCodes.TypeMismatch, "Cannot negate non-number", "number", GetTypeName(value));
        }
        return CreateNumber(-value.GetDouble());
    }

    private static JsonElement EvaluatePositive(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Number)
        {
            throw new JsonEException(JsonEErrorCodes.TypeMismatch, "Expected number for unary +", "number", GetTypeName(value));
        }
        return value;
    }

    private static bool IsTruthy(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => false,
            JsonValueKind.Undefined => false,
            JsonValueKind.Number => value.GetDouble() != 0,
            JsonValueKind.String => !string.IsNullOrEmpty(value.GetString()),
            JsonValueKind.Array => value.GetArrayLength() > 0,
            JsonValueKind.Object => true,
            _ => false
        };
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

    private static JsonElement CreateBool(bool value)
    {
        using var doc = JsonDocument.Parse(value ? "true" : "false");
        return doc.RootElement.Clone();
    }

    private static JsonElement CreateNumber(double value)
    {
        var json = JsonSerializer.Serialize(value);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }
}
