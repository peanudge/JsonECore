using System.Text.Json;
using JsonECore.Context;
using static JsonECore.JsonElementHelper;

namespace JsonECore.Functions;

/// <summary>
/// Math-related built-in functions.
/// </summary>
public static class MathFunctions
{
    private static double GetNumberArg(JsonElement value, string funcName)
    {
        if (value.ValueKind != JsonValueKind.Number)
        {
            throw new JsonEException(JsonEErrorCodes.TypeMismatch, $"{funcName}() requires number arguments", "number", GetTypeName(value));
        }
        return value.GetDouble();
    }

    public class MinFunction : IBuiltInFunction
    {
        public string Name => "min";

        public JsonElement Execute(List<JsonElement> args, EvaluationContext context)
        {
            if (args.Count == 0)
            {
                throw new JsonEException(JsonEErrorCodes.InvalidFunctionCall, "min() requires at least one argument", Name);
            }

            double minValue = double.PositiveInfinity;
            foreach (var arg in args)
            {
                var value = GetNumberArg(arg, Name);
                if (value < minValue)
                {
                    minValue = value;
                }
            }

            return CreateNumber(minValue);
        }
    }

    public class MaxFunction : IBuiltInFunction
    {
        public string Name => "max";

        public JsonElement Execute(List<JsonElement> args, EvaluationContext context)
        {
            if (args.Count == 0)
            {
                throw new JsonEException(JsonEErrorCodes.InvalidFunctionCall, "max() requires at least one argument", Name);
            }

            double maxValue = double.NegativeInfinity;
            foreach (var arg in args)
            {
                var value = GetNumberArg(arg, Name);
                if (value > maxValue)
                {
                    maxValue = value;
                }
            }

            return CreateNumber(maxValue);
        }
    }

    public class SqrtFunction : IBuiltInFunction
    {
        public string Name => "sqrt";

        public JsonElement Execute(List<JsonElement> args, EvaluationContext context)
        {
            if (args.Count != 1)
            {
                throw new JsonEException(JsonEErrorCodes.InvalidFunctionCall, "sqrt() requires exactly one argument", Name);
            }

            var value = GetNumberArg(args[0], Name);
            return CreateNumber(Math.Sqrt(value));
        }
    }

    public class CeilFunction : IBuiltInFunction
    {
        public string Name => "ceil";

        public JsonElement Execute(List<JsonElement> args, EvaluationContext context)
        {
            if (args.Count != 1)
            {
                throw new JsonEException(JsonEErrorCodes.InvalidFunctionCall, "ceil() requires exactly one argument", Name);
            }

            var value = GetNumberArg(args[0], Name);
            return CreateNumber(Math.Ceiling(value));
        }
    }

    public class FloorFunction : IBuiltInFunction
    {
        public string Name => "floor";

        public JsonElement Execute(List<JsonElement> args, EvaluationContext context)
        {
            if (args.Count != 1)
            {
                throw new JsonEException(JsonEErrorCodes.InvalidFunctionCall, "floor() requires exactly one argument", Name);
            }

            var value = GetNumberArg(args[0], Name);
            return CreateNumber(Math.Floor(value));
        }
    }

    public class AbsFunction : IBuiltInFunction
    {
        public string Name => "abs";

        public JsonElement Execute(List<JsonElement> args, EvaluationContext context)
        {
            if (args.Count != 1)
            {
                throw new JsonEException(JsonEErrorCodes.InvalidFunctionCall, "abs() requires exactly one argument", Name);
            }

            var value = GetNumberArg(args[0], Name);
            return CreateNumber(Math.Abs(value));
        }
    }
}
