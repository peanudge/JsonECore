using System.Text.Json;
using JsonECore.Context;
using static JsonECore.JsonElementHelper;

namespace JsonECore.Expressions.Ast;

/// <summary>
/// Represents a binary operation (e.g., a + b, a AND b).
/// </summary>
public class BinaryExpression : IExpression
{
    public IExpression Left { get; }
    public string Operator { get; }
    public IExpression Right { get; }

    public BinaryExpression(IExpression left, string op, IExpression right)
    {
        Left = left;
        Operator = op;
        Right = right;
    }

    public JsonElement Evaluate(EvaluationContext context)
    {
        // Short-circuit evaluation for logical operators
        if (Operator == "&&")
        {
            var leftVal = Left.Evaluate(context);
            if (!IsTruthy(leftVal)) return CreateBool(false);
            return CreateBool(IsTruthy(Right.Evaluate(context)));
        }

        if (Operator == "||")
        {
            var leftVal = Left.Evaluate(context);
            if (IsTruthy(leftVal)) return CreateBool(true);
            return CreateBool(IsTruthy(Right.Evaluate(context)));
        }

        var left = Left.Evaluate(context);
        var right = Right.Evaluate(context);

        return Operator switch
        {
            "+" => EvaluateAdd(left, right),
            "-" => CreateNumber(GetNumber(left) - GetNumber(right)),
            "*" => CreateNumber(GetNumber(left) * GetNumber(right)),
            "/" => EvaluateDivide(left, right),
            "%" => EvaluateModulo(left, right),
            "**" => CreateNumber(Math.Pow(GetNumber(left), GetNumber(right))),
            "==" => CreateBool(AreEqual(left, right)),
            "!=" => CreateBool(!AreEqual(left, right)),
            "<" => CreateBool(Compare(left, right) < 0),
            "<=" => CreateBool(Compare(left, right) <= 0),
            ">" => CreateBool(Compare(left, right) > 0),
            ">=" => CreateBool(Compare(left, right) >= 0),
            "in" => EvaluateIn(left, right),
            _ => throw new JsonEException(JsonEErrorCodes.SyntaxError, $"Unknown operator: {Operator}", 0, Operator)
        };
    }

    private static JsonElement EvaluateAdd(JsonElement left, JsonElement right)
    {
        if (IsString(left) || IsString(right))
        {
            return CreateString(ConvertToString(left) + ConvertToString(right));
        }

        if (IsArray(left) && IsArray(right))
        {
            var result = left.EnumerateArray().Select(x => x.Clone())
                .Concat(right.EnumerateArray().Select(x => x.Clone()));
            return CreateArray(result);
        }

        return CreateNumber(GetNumber(left) + GetNumber(right));
    }

    private static JsonElement EvaluateDivide(JsonElement left, JsonElement right)
    {
        var rightNum = GetNumber(right);
        if (rightNum == 0)
            throw new JsonEException(JsonEErrorCodes.DivisionByZero, "Division by zero");
        return CreateNumber(GetNumber(left) / rightNum);
    }

    private static JsonElement EvaluateModulo(JsonElement left, JsonElement right)
    {
        var rightNum = GetNumber(right);
        if (rightNum == 0)
            throw new JsonEException(JsonEErrorCodes.DivisionByZero, "Division by zero");
        return CreateNumber(GetNumber(left) % rightNum);
    }

    private static JsonElement EvaluateIn(JsonElement left, JsonElement right)
    {
        if (IsArray(right))
        {
            foreach (var item in right.EnumerateArray())
            {
                if (AreEqual(left, item)) return CreateBool(true);
            }
            return CreateBool(false);
        }

        if (IsObject(right))
        {
            if (!IsString(left))
                throw new JsonEException(JsonEErrorCodes.TypeMismatch, "The 'in' operator requires a string key for objects", "string", GetTypeName(left));
            return CreateBool(right.TryGetProperty(left.GetString()!, out _));
        }

        if (IsString(right) && IsString(left))
        {
            return CreateBool(right.GetString()!.Contains(left.GetString()!));
        }

        throw new JsonEException(JsonEErrorCodes.TypeMismatch, "The 'in' operator requires an array, object, or string on the right side", "array/object/string", GetTypeName(right));
    }
}
