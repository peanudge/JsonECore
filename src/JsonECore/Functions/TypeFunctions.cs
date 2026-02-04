using System.Text.Json;
using JsonECore.Context;

namespace JsonECore.Functions;

/// <summary>
/// Type-related built-in functions.
/// </summary>
public static class TypeFunctions
{
    private static JsonElement CreateString(string value)
    {
        var json = JsonSerializer.Serialize(value);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    private static JsonElement CreateNumber(double value)
    {
        var json = JsonSerializer.Serialize(value);
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

    public class TypeofFunction : IBuiltInFunction
    {
        public string Name => "typeof";

        public JsonElement Execute(List<JsonElement> args, EvaluationContext context)
        {
            if (args.Count != 1)
            {
                throw new JsonEException(JsonEErrorCodes.InvalidFunctionCall, "typeof() requires exactly one argument", Name);
            }

            return CreateString(GetTypeName(args[0]));
        }
    }

    public class StrFunction : IBuiltInFunction
    {
        public string Name => "str";

        public JsonElement Execute(List<JsonElement> args, EvaluationContext context)
        {
            if (args.Count != 1)
            {
                throw new JsonEException(JsonEErrorCodes.InvalidFunctionCall, "str() requires exactly one argument", Name);
            }

            var value = args[0];
            var str = value.ValueKind switch
            {
                JsonValueKind.String => value.GetString()!,
                JsonValueKind.Number => FormatNumber(value.GetDouble()),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Null => "null",
                _ => value.GetRawText()
            };

            return CreateString(str);
        }

        private static string FormatNumber(double value)
        {
            if (value == Math.Truncate(value) && !double.IsInfinity(value))
            {
                return ((long)value).ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            return value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    public class NumberFunction : IBuiltInFunction
    {
        public string Name => "number";

        public JsonElement Execute(List<JsonElement> args, EvaluationContext context)
        {
            if (args.Count != 1)
            {
                throw new JsonEException(JsonEErrorCodes.InvalidFunctionCall, "number() requires exactly one argument", Name);
            }

            var value = args[0];
            double result = value.ValueKind switch
            {
                JsonValueKind.Number => value.GetDouble(),
                JsonValueKind.String => ParseNumber(value.GetString()!),
                JsonValueKind.True => 1,
                JsonValueKind.False => 0,
                _ => throw new JsonEException(JsonEErrorCodes.TypeMismatch, $"Cannot convert {GetTypeName(value)} to number", "number", GetTypeName(value))
            };

            return CreateNumber(result);
        }

        private static double ParseNumber(string str)
        {
            if (double.TryParse(str, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }
            throw new JsonEException(JsonEErrorCodes.TypeMismatch, $"Cannot convert string '{str}' to number", "number", "string");
        }
    }

    public class LenFunction : IBuiltInFunction
    {
        public string Name => "len";

        public JsonElement Execute(List<JsonElement> args, EvaluationContext context)
        {
            if (args.Count != 1)
            {
                throw new JsonEException(JsonEErrorCodes.InvalidFunctionCall, "len() requires exactly one argument", Name);
            }

            var value = args[0];
            int length = value.ValueKind switch
            {
                JsonValueKind.String => value.GetString()!.Length,
                JsonValueKind.Array => value.GetArrayLength(),
                JsonValueKind.Object => value.EnumerateObject().Count(),
                _ => throw new JsonEException(JsonEErrorCodes.TypeMismatch, $"len() cannot be applied to {GetTypeName(value)}", "string/array/object", GetTypeName(value))
            };

            return CreateNumber(length);
        }
    }
}
