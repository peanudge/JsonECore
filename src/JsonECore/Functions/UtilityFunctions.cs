using System.Text.Json;
using System.Text.RegularExpressions;
using JsonECore.Context;
using static JsonECore.JsonElementHelper;

namespace JsonECore.Functions;

/// <summary>
/// Utility built-in functions.
/// </summary>
public static class UtilityFunctions
{
    public class RangeFunction : IBuiltInFunction
    {
        public string Name => "range";

        public JsonElement Execute(List<JsonElement> args, EvaluationContext context)
        {
            if (args.Count < 1 || args.Count > 3)
            {
                throw new JsonEException(JsonEErrorCodes.InvalidFunctionCall, "range() requires 1 to 3 arguments", Name);
            }

            int start, end, step;

            if (args.Count == 1)
            {
                start = 0;
                end = GetIntArg(args[0], Name);
                step = 1;
            }
            else if (args.Count == 2)
            {
                start = GetIntArg(args[0], Name);
                end = GetIntArg(args[1], Name);
                step = 1;
            }
            else
            {
                start = GetIntArg(args[0], Name);
                end = GetIntArg(args[1], Name);
                step = GetIntArg(args[2], Name);
            }

            if (step == 0)
            {
                throw new JsonEException(JsonEErrorCodes.InvalidArgument, "range() step cannot be zero", Name);
            }

            var result = new List<int>();
            if (step > 0)
            {
                for (int i = start; i < end; i += step)
                {
                    result.Add(i);
                }
            }
            else
            {
                for (int i = start; i > end; i += step)
                {
                    result.Add(i);
                }
            }

            return CreateArray(result);
        }

        private static int GetIntArg(JsonElement value, string funcName)
        {
            if (value.ValueKind != JsonValueKind.Number)
            {
                throw new JsonEException(JsonEErrorCodes.TypeMismatch, $"{funcName}() requires integer arguments", "number", GetTypeName(value));
            }
            return (int)value.GetDouble();
        }
    }

    public class DefinedFunction : IBuiltInFunction
    {
        public string Name => "defined";

        public JsonElement Execute(List<JsonElement> args, EvaluationContext context)
        {
            if (args.Count != 1)
            {
                throw new JsonEException(JsonEErrorCodes.InvalidFunctionCall, "defined() requires exactly one argument", Name);
            }

            if (args[0].ValueKind != JsonValueKind.String)
            {
                throw new JsonEException(JsonEErrorCodes.TypeMismatch, "defined() requires a string argument", "string", GetTypeName(args[0]));
            }

            var varName = args[0].GetString()!;
            return CreateBool(context.HasVariable(varName));
        }
    }

    public class FromNowFunction : IBuiltInFunction
    {
        public string Name => "fromNow";

        // Matches single duration components like "1d", "-2 hours", etc.
        private static readonly Regex DurationComponentRegex = new(
            @"(-?\d+)\s*(years?|months?|weeks?|days?|hours?|minutes?|mins?|seconds?|secs?|s|m|h|d|w|mo|y)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public JsonElement Execute(List<JsonElement> args, EvaluationContext context)
        {
            if (args.Count < 1 || args.Count > 2)
            {
                throw new JsonEException(JsonEErrorCodes.InvalidFunctionCall, "fromNow() requires 1 or 2 arguments", Name);
            }

            if (args[0].ValueKind != JsonValueKind.String)
            {
                throw new JsonEException(JsonEErrorCodes.TypeMismatch, "fromNow() first argument must be a string", "string", GetTypeName(args[0]));
            }

            var duration = args[0].GetString()!;
            DateTime baseTime;

            if (args.Count == 2)
            {
                if (args[1].ValueKind != JsonValueKind.String)
                {
                    throw new JsonEException(JsonEErrorCodes.TypeMismatch, "fromNow() second argument must be a string", "string", GetTypeName(args[1]));
                }
                baseTime = DateTime.Parse(args[1].GetString()!, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind);
            }
            else
            {
                baseTime = DateTime.UtcNow;
            }

            var result = AddDuration(baseTime, duration);
            return CreateString(result.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
        }

        private DateTime AddDuration(DateTime baseTime, string duration)
        {
            var matches = DurationComponentRegex.Matches(duration.Trim());
            if (matches.Count == 0)
            {
                throw new JsonEException(JsonEErrorCodes.InvalidDateTime, $"Invalid duration format: {duration}", duration);
            }

            var result = baseTime;
            foreach (Match match in matches)
            {
                var amount = int.Parse(match.Groups[1].Value);
                var unit = match.Groups[2].Value.ToLowerInvariant();

                result = unit switch
                {
                    "s" or "sec" or "secs" or "second" or "seconds" => result.AddSeconds(amount),
                    "m" or "min" or "mins" or "minute" or "minutes" => result.AddMinutes(amount),
                    "h" or "hour" or "hours" => result.AddHours(amount),
                    "d" or "day" or "days" => result.AddDays(amount),
                    "w" or "week" or "weeks" => result.AddDays(amount * 7),
                    "mo" or "month" or "months" => result.AddMonths(amount),
                    "y" or "year" or "years" => result.AddYears(amount),
                    _ => throw new JsonEException(JsonEErrorCodes.InvalidDateTime, $"Unknown duration unit: {unit}", unit)
                };
            }

            return result;
        }
    }
}
