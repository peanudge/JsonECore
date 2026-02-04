using System.Text.Json;
using JsonECore.Context;

namespace JsonECore.Expressions.Ast;

/// <summary>
/// Represents a variable identifier.
/// </summary>
public class IdentifierExpression : IExpression
{
    public string Name { get; }

    public IdentifierExpression(string name)
    {
        Name = name;
    }

    public JsonElement Evaluate(EvaluationContext context)
    {
        return context.GetVariable(Name);
    }
}
