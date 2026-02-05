using System.Text.Json;
using JsonECore.Context;
using static JsonECore.JsonElementHelper;

namespace JsonECore.Expressions.Ast;

/// <summary>
/// Represents an array literal expression (e.g., [1, 2, 3]).
/// </summary>
public class ArrayExpression : IExpression
{
    public List<IExpression> Elements { get; }

    public ArrayExpression(List<IExpression> elements)
    {
        Elements = elements;
    }

    public JsonElement Evaluate(EvaluationContext context)
    {
        var result = Elements.Select(e => e.Evaluate(context).Clone());
        return CreateArray(result);
    }
}
