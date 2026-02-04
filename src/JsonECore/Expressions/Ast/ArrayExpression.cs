using System.Text.Json;
using JsonECore.Context;

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
        var result = new List<JsonElement>();
        foreach (var element in Elements)
        {
            result.Add(element.Evaluate(context).Clone());
        }

        var json = JsonSerializer.Serialize(result);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }
}
