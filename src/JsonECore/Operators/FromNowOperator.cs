using System.Text.Json;
using System.Text.RegularExpressions;
using JsonECore.Context;

namespace JsonECore.Operators;

/// <summary>
/// Implements the $fromNow operator.
/// </summary>
public class FromNowOperator : IOperator
{
    public string Name => "$fromNow";

    private static readonly Regex DurationRegex = new(
        @"^(-?\d+)\s*(years?|months?|weeks?|days?|hours?|minutes?|mins?|seconds?|secs?|s|m|h|d|w|mo|y)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public JsonElement Execute(JsonElement template, EvaluationContext context, Func<JsonElement, EvaluationContext, JsonElement> render)
    {
        if (!template.TryGetProperty("$fromNow", out var duration))
        {
            throw new JsonEException(JsonEErrorCodes.InvalidTemplate, "$fromNow requires duration string", Name);
        }

        var durationValue = render(duration, context);

        if (durationValue.ValueKind != JsonValueKind.String)
        {
            throw new JsonEException(JsonEErrorCodes.TypeMismatch, "$fromNow duration must be a string", "string", GetTypeName(durationValue));
        }

        DateTime baseTime;
        if (template.TryGetProperty("from", out var fromValue))
        {
            var fromRendered = render(fromValue, context);
            if (fromRendered.ValueKind != JsonValueKind.String)
            {
                throw new JsonEException(JsonEErrorCodes.TypeMismatch, "$fromNow 'from' must be a string", "string", GetTypeName(fromRendered));
            }
            baseTime = DateTime.Parse(fromRendered.GetString()!, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind);
        }
        else
        {
            baseTime = DateTime.UtcNow;
        }

        var result = AddDuration(baseTime, durationValue.GetString()!);
        return CreateString(result.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
    }

    private DateTime AddDuration(DateTime baseTime, string duration)
    {
        var match = DurationRegex.Match(duration.Trim());
        if (!match.Success)
        {
            throw new JsonEException(JsonEErrorCodes.InvalidDateTime, $"Invalid duration format: {duration}", duration);
        }

        var amount = int.Parse(match.Groups[1].Value);
        var unit = match.Groups[2].Value.ToLowerInvariant();

        return unit switch
        {
            "s" or "sec" or "secs" or "second" or "seconds" => baseTime.AddSeconds(amount),
            "m" or "min" or "mins" or "minute" or "minutes" => baseTime.AddMinutes(amount),
            "h" or "hour" or "hours" => baseTime.AddHours(amount),
            "d" or "day" or "days" => baseTime.AddDays(amount),
            "w" or "week" or "weeks" => baseTime.AddDays(amount * 7),
            "mo" or "month" or "months" => baseTime.AddMonths(amount),
            "y" or "year" or "years" => baseTime.AddYears(amount),
            _ => throw new JsonEException(JsonEErrorCodes.InvalidDateTime, $"Unknown duration unit: {unit}", unit)
        };
    }

    private static JsonElement CreateString(string value)
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
}
