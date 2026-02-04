using System.Text.Json;
using JsonECore.Context;

namespace JsonECore.Expressions.Ast;

/// <summary>
/// Represents a ternary conditional expression (e.g., a ? b : c).
/// </summary>
public class ConditionalExpression : IExpression
{
    public IExpression Condition { get; }
    public IExpression TrueExpression { get; }
    public IExpression FalseExpression { get; }

    public ConditionalExpression(IExpression condition, IExpression trueExpr, IExpression falseExpr)
    {
        Condition = condition;
        TrueExpression = trueExpr;
        FalseExpression = falseExpr;
    }

    public JsonElement Evaluate(EvaluationContext context)
    {
        var conditionResult = Condition.Evaluate(context);

        if (IsTruthy(conditionResult))
        {
            return TrueExpression.Evaluate(context);
        }
        else
        {
            return FalseExpression.Evaluate(context);
        }
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
}
