using System.Text.Json;
using JsonECore.Context;
using static JsonECore.JsonElementHelper;

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
        return IsTruthy(Condition.Evaluate(context))
            ? TrueExpression.Evaluate(context)
            : FalseExpression.Evaluate(context);
    }
}
