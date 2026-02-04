using System.Text.Json;
using System.Text.RegularExpressions;
using JsonECore.Context;
using JsonECore.Expressions.Ast;

namespace JsonECore.Expressions;

/// <summary>
/// Evaluates JSON-E expressions, including string interpolation.
/// </summary>
public class ExpressionEvaluator
{
    private static readonly Regex InterpolationRegex = new(@"\$\{([^}]+)\}", RegexOptions.Compiled);

    /// <summary>
    /// Evaluates an expression string and returns the result.
    /// </summary>
    public static JsonElement Evaluate(string expression, EvaluationContext context)
    {
        var ast = ExpressionParser.Parse(expression);
        return ast.Evaluate(context);
    }

    /// <summary>
    /// Evaluates a string with interpolation (e.g., "Hello ${name}!").
    /// </summary>
    public static string EvaluateInterpolation(string template, EvaluationContext context)
    {
        return InterpolationRegex.Replace(template, match =>
        {
            var expression = match.Groups[1].Value;
            var result = Evaluate(expression, context);
            return ConvertToString(result);
        });
    }

    /// <summary>
    /// Checks if a string contains interpolation patterns.
    /// </summary>
    public static bool HasInterpolation(string value)
    {
        return InterpolationRegex.IsMatch(value);
    }

    /// <summary>
    /// Checks if the value is truthy according to JSON-E semantics.
    /// </summary>
    public static bool IsTruthy(JsonElement value)
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
        // Format integers without decimal point
        if (value == Math.Truncate(value) && !double.IsInfinity(value))
        {
            return ((long)value).ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
        return value.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
}
