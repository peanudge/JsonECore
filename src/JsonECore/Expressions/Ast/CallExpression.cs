using System.Text.Json;
using JsonECore.Context;
using JsonECore.Functions;

namespace JsonECore.Expressions.Ast;

/// <summary>
/// Represents a function call (e.g., min(a, b)).
/// </summary>
public class CallExpression : IExpression
{
    public string FunctionName { get; }
    public List<IExpression> Arguments { get; }

    public CallExpression(string functionName, List<IExpression> arguments)
    {
        FunctionName = functionName;
        Arguments = arguments;
    }

    public JsonElement Evaluate(EvaluationContext context)
    {
        var args = Arguments.Select(arg => arg.Evaluate(context)).ToList();
        return FunctionRegistry.Instance.Call(FunctionName, args, context);
    }
}
