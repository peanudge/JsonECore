using System.Text.Json;
using JsonECore.Context;

namespace JsonECore.Expressions.Ast;

/// <summary>
/// Base interface for all expression AST nodes.
/// </summary>
public interface IExpression
{
    JsonElement Evaluate(EvaluationContext context);
}
