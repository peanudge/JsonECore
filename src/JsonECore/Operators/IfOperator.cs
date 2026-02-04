using System.Text.Json;
using JsonECore.Context;
using JsonECore.Expressions;

namespace JsonECore.Operators;

/// <summary>
/// Implements the $if operator.
/// </summary>
public class IfOperator : IOperator
{
    public string Name => "$if";

    public JsonElement Execute(JsonElement template, EvaluationContext context, Func<JsonElement, EvaluationContext, JsonElement> render)
    {
        if (!template.TryGetProperty("$if", out var condition))
        {
            throw new JsonEException(JsonEErrorCodes.InvalidTemplate, "$if requires condition", Name);
        }

        bool conditionResult;
        if (condition.ValueKind == JsonValueKind.String)
        {
            var result = ExpressionEvaluator.Evaluate(condition.GetString()!, context);
            conditionResult = ExpressionEvaluator.IsTruthy(result);
        }
        else
        {
            conditionResult = ExpressionEvaluator.IsTruthy(condition);
        }

        if (conditionResult)
        {
            if (template.TryGetProperty("then", out var thenValue))
            {
                return render(thenValue, context);
            }
            return CreateDeleteMarker();
        }
        else
        {
            if (template.TryGetProperty("else", out var elseValue))
            {
                return render(elseValue, context);
            }
            return CreateDeleteMarker();
        }
    }

    private static JsonElement CreateDeleteMarker()
    {
        return DeleteMarker.Create();
    }
}
