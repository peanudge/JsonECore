using System.Text.Json;
using JsonECore.Context;
using static JsonECore.JsonElementHelper;

namespace JsonECore.Functions;

/// <summary>
/// String-related built-in functions.
/// </summary>
public static class StringFunctions
{
    private static string GetStringArg(JsonElement value, string funcName)
    {
        if (value.ValueKind != JsonValueKind.String)
        {
            throw new JsonEException(JsonEErrorCodes.TypeMismatch, $"{funcName}() requires string argument", "string", GetTypeName(value));
        }
        return value.GetString()!;
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

            var str = GetStringArg(args[0], Name);
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

            var str = GetStringArg(args[0], Name);
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

            var str = GetStringArg(args[0], Name);
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

            var str = GetStringArg(args[0], Name);
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

            var str = GetStringArg(args[0], Name);
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

            var str = GetStringArg(args[0], Name);
            var delimiter = GetStringArg(args[1], Name);
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

            var separator = GetStringArg(args[1], Name);
            var items = new List<string>();
            foreach (var item in args[0].EnumerateArray())
            {
                items.Add(ConvertToString(item));
            }

            return CreateString(string.Join(separator, items));
        }
    }
}
