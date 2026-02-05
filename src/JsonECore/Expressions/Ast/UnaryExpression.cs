using System.Text.Json;
using JsonECore.Context;
using static JsonECore.JsonElementHelper;

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
            "!" => CreateBool(!IsTruthy(value)),
            "-" => CreateNumber(-GetNumber(value, "unary -")),
            "+" => IsNumber(value) ? value : throw new JsonEException(JsonEErrorCodes.TypeMismatch, "Expected number for unary +", "number", GetTypeName(value)),
            _ => throw new JsonEException(JsonEErrorCodes.SyntaxError, $"Unknown unary operator: {Operator}", 0, Operator)
        };
    }
}
