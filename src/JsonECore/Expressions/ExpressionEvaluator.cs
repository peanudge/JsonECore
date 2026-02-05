using System.Text.Json;
using System.Text.RegularExpressions;
using JsonECore.Context;
using JsonECore.Expressions.Ast;
using static JsonECore.JsonElementHelper;

namespace JsonECore.Expressions;

/// <summary>
/// Evaluates JSON-E expressions, including string interpolation.
/// </summary>
public static class ExpressionEvaluator
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
    public static bool IsTruthy(JsonElement value) => JsonElementHelper.IsTruthy(value);
}
