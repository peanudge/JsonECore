using System.Text.Json;
using JsonECore.Context;

namespace JsonECore.Functions;

/// <summary>
/// String-related built-in functions.
/// </summary>
public static class StringFunctions
{
    private static JsonElement CreateString(string value)
    {
        var json = JsonSerializer.Serialize(value);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    private static JsonElement CreateArray(List<string> items)
    {
        var json = JsonSerializer.Serialize(items);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    private static string GetString(JsonElement value, string funcName)
    {
        if (value.ValueKind != JsonValueKind.String)
        {
            throw new JsonEException(JsonEErrorCodes.TypeMismatch, $"{funcName}() requires string argument", "string", GetTypeName(value));
        }
        return value.GetString()!;
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

    public class LowercaseFunction : IBuiltInFunction
    {
        public string Name => "lowercase";

        public JsonElement Execute(List<JsonElement> args, EvaluationContext context)
        {
            if (args.Count != 1)
            {
                throw new JsonEException(JsonEErrorCodes.InvalidFunctionCall, "lowercase() requires exactly one argument", Name);
            }

            var str = GetString(args[0], Name);
            return CreateString(str.ToLowerInvariant());
        }
    }

    public class UppercaseFunction : IBuiltInFunction
    {
        public string Name => "uppercase";

        public JsonElement Execute(List<JsonElement> args, EvaluationContext context)
        {
            if (args.Count != 1)
            {
                throw new JsonEException(JsonEErrorCodes.InvalidFunctionCall, "uppercase() requires exactly one argument", Name);
            }

            var str = GetString(args[0], Name);
            return CreateString(str.ToUpperInvariant());
        }
    }

    public class StripFunction : IBuiltInFunction
    {
        public string Name => "strip";

        public JsonElement Execute(List<JsonElement> args, EvaluationContext context)
        {
            if (args.Count != 1)
            {
                throw new JsonEException(JsonEErrorCodes.InvalidFunctionCall, "strip() requires exactly one argument", Name);
            }

            var str = GetString(args[0], Name);
            return CreateString(str.Trim());
        }
    }

    public class LstripFunction : IBuiltInFunction
    {
        public string Name => "lstrip";

        public JsonElement Execute(List<JsonElement> args, EvaluationContext context)
        {
            if (args.Count != 1)
            {
                throw new JsonEException(JsonEErrorCodes.InvalidFunctionCall, "lstrip() requires exactly one argument", Name);
            }

            var str = GetString(args[0], Name);
            return CreateString(str.TrimStart());
        }
    }

    public class RstripFunction : IBuiltInFunction
    {
        public string Name => "rstrip";

        public JsonElement Execute(List<JsonElement> args, EvaluationContext context)
        {
            if (args.Count != 1)
            {
                throw new JsonEException(JsonEErrorCodes.InvalidFunctionCall, "rstrip() requires exactly one argument", Name);
            }

            var str = GetString(args[0], Name);
            return CreateString(str.TrimEnd());
        }
    }

    public class SplitFunction : IBuiltInFunction
    {
        public string Name => "split";

        public JsonElement Execute(List<JsonElement> args, EvaluationContext context)
        {
            if (args.Count != 2)
            {
                throw new JsonEException(JsonEErrorCodes.InvalidFunctionCall, "split() requires exactly two arguments", Name);
            }

            var str = GetString(args[0], Name);
            var delimiter = GetString(args[1], Name);
            var parts = str.Split(delimiter);
            return CreateArray(parts.ToList());
        }
    }

    public class JoinFunction : IBuiltInFunction
    {
        public string Name => "join";

        public JsonElement Execute(List<JsonElement> args, EvaluationContext context)
        {
            if (args.Count != 2)
            {
                throw new JsonEException(JsonEErrorCodes.InvalidFunctionCall, "join() requires exactly two arguments", Name);
            }

            if (args[0].ValueKind != JsonValueKind.Array)
            {
                throw new JsonEException(JsonEErrorCodes.TypeMismatch, "join() first argument must be an array", "array", GetTypeName(args[0]));
            }

            var separator = GetString(args[1], Name);
            var items = new List<string>();
            foreach (var item in args[0].EnumerateArray())
            {
                items.Add(ConvertToString(item));
            }

            return CreateString(string.Join(separator, items));
        }

        private static string ConvertToString(JsonElement value)
        {
            return value.ValueKind switch
            {
                JsonValueKind.String => value.GetString() ?? "",
                JsonValueKind.Number => FormatNumber(value.GetDouble()),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Null => "null",
                _ => value.GetRawText()
            };
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
}
