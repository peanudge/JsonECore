using System.Text.Json;
using JsonECore.Context;

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
            if (!IsTruthy(leftVal))
            {
                return CreateBool(false);
            }
            var rightVal = Right.Evaluate(context);
            return CreateBool(IsTruthy(rightVal));
        }

        if (Operator == "||")
        {
            var leftVal = Left.Evaluate(context);
            if (IsTruthy(leftVal))
            {
                return CreateBool(true);
            }
            var rightVal = Right.Evaluate(context);
            return CreateBool(IsTruthy(rightVal));
        }

        var left = Left.Evaluate(context);
        var right = Right.Evaluate(context);

        return Operator switch
        {
            "+" => EvaluateAdd(left, right),
            "-" => EvaluateSubtract(left, right),
            "*" => EvaluateMultiply(left, right),
            "/" => EvaluateDivide(left, right),
            "%" => EvaluateModulo(left, right),
            "**" => EvaluatePower(left, right),
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
        if (left.ValueKind == JsonValueKind.String || right.ValueKind == JsonValueKind.String)
        {
            var leftStr = ConvertToString(left);
            var rightStr = ConvertToString(right);
            return CreateString(leftStr + rightStr);
        }

        if (left.ValueKind == JsonValueKind.Array && right.ValueKind == JsonValueKind.Array)
        {
            var result = new List<JsonElement>();
            foreach (var item in left.EnumerateArray())
            {
                result.Add(item.Clone());
            }
            foreach (var item in right.EnumerateArray())
            {
                result.Add(item.Clone());
            }
            return CreateArray(result);
        }

        var leftNum = GetNumber(left);
        var rightNum = GetNumber(right);
        return CreateNumber(leftNum + rightNum);
    }

    private static JsonElement EvaluateSubtract(JsonElement left, JsonElement right)
    {
        var leftNum = GetNumber(left);
        var rightNum = GetNumber(right);
        return CreateNumber(leftNum - rightNum);
    }

    private static JsonElement EvaluateMultiply(JsonElement left, JsonElement right)
    {
        var leftNum = GetNumber(left);
        var rightNum = GetNumber(right);
        return CreateNumber(leftNum * rightNum);
    }

    private static JsonElement EvaluateDivide(JsonElement left, JsonElement right)
    {
        var leftNum = GetNumber(left);
        var rightNum = GetNumber(right);
        if (rightNum == 0)
        {
            throw new JsonEException(JsonEErrorCodes.DivisionByZero, "Division by zero");
        }
        return CreateNumber(leftNum / rightNum);
    }

    private static JsonElement EvaluateModulo(JsonElement left, JsonElement right)
    {
        var leftNum = GetNumber(left);
        var rightNum = GetNumber(right);
        if (rightNum == 0)
        {
            throw new JsonEException(JsonEErrorCodes.DivisionByZero, "Division by zero");
        }
        return CreateNumber(leftNum % rightNum);
    }

    private static JsonElement EvaluatePower(JsonElement left, JsonElement right)
    {
        var leftNum = GetNumber(left);
        var rightNum = GetNumber(right);
        return CreateNumber(Math.Pow(leftNum, rightNum));
    }

    private static JsonElement EvaluateIn(JsonElement left, JsonElement right)
    {
        if (right.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in right.EnumerateArray())
            {
                if (AreEqual(left, item))
                {
                    return CreateBool(true);
                }
            }
            return CreateBool(false);
        }

        if (right.ValueKind == JsonValueKind.Object)
        {
            if (left.ValueKind != JsonValueKind.String)
            {
                throw new JsonEException(JsonEErrorCodes.TypeMismatch, "The 'in' operator requires a string key for objects", "string", GetTypeName(left));
            }
            return CreateBool(right.TryGetProperty(left.GetString()!, out _));
        }

        if (right.ValueKind == JsonValueKind.String && left.ValueKind == JsonValueKind.String)
        {
            return CreateBool(right.GetString()!.Contains(left.GetString()!));
        }

        throw new JsonEException(JsonEErrorCodes.TypeMismatch, "The 'in' operator requires an array, object, or string on the right side", "array/object/string", GetTypeName(right));
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

    private static bool AreEqual(JsonElement left, JsonElement right)
    {
        if (left.ValueKind != right.ValueKind)
        {
            // Handle number comparisons across types
            if (IsNumber(left) && IsNumber(right))
            {
                return GetNumber(left) == GetNumber(right);
            }
            return false;
        }

        return left.ValueKind switch
        {
            JsonValueKind.Null => true,
            JsonValueKind.True => true,
            JsonValueKind.False => true,
            JsonValueKind.Number => left.GetDouble() == right.GetDouble(),
            JsonValueKind.String => left.GetString() == right.GetString(),
            JsonValueKind.Array => ArraysEqual(left, right),
            JsonValueKind.Object => ObjectsEqual(left, right),
            _ => false
        };
    }

    private static bool ArraysEqual(JsonElement left, JsonElement right)
    {
        if (left.GetArrayLength() != right.GetArrayLength())
            return false;

        var leftArr = left.EnumerateArray().ToList();
        var rightArr = right.EnumerateArray().ToList();

        for (int i = 0; i < leftArr.Count; i++)
        {
            if (!AreEqual(leftArr[i], rightArr[i]))
                return false;
        }
        return true;
    }

    private static bool ObjectsEqual(JsonElement left, JsonElement right)
    {
        var leftProps = left.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);
        var rightProps = right.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);

        if (leftProps.Count != rightProps.Count)
            return false;

        foreach (var kvp in leftProps)
        {
            if (!rightProps.TryGetValue(kvp.Key, out var rightVal))
                return false;
            if (!AreEqual(kvp.Value, rightVal))
                return false;
        }
        return true;
    }

    private static int Compare(JsonElement left, JsonElement right)
    {
        if (IsNumber(left) && IsNumber(right))
        {
            return GetNumber(left).CompareTo(GetNumber(right));
        }

        if (left.ValueKind == JsonValueKind.String && right.ValueKind == JsonValueKind.String)
        {
            return string.Compare(left.GetString(), right.GetString(), StringComparison.Ordinal);
        }

        throw new JsonEException(JsonEErrorCodes.TypeMismatch, "Cannot compare values of different types", GetTypeName(left), GetTypeName(right));
    }

    private static bool IsNumber(JsonElement value)
    {
        return value.ValueKind == JsonValueKind.Number;
    }

    private static double GetNumber(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Number)
        {
            return value.GetDouble();
        }
        throw new JsonEException(JsonEErrorCodes.TypeMismatch, $"Expected number, got {GetTypeName(value)}", "number", GetTypeName(value));
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

    private static string ConvertToString(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString() ?? "",
            JsonValueKind.Number => value.GetDouble().ToString(System.Globalization.CultureInfo.InvariantCulture),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => "null",
            _ => value.GetRawText()
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

    private static JsonElement CreateString(string value)
    {
        var json = JsonSerializer.Serialize(value);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    private static JsonElement CreateArray(List<JsonElement> items)
    {
        var json = JsonSerializer.Serialize(items);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }
}
