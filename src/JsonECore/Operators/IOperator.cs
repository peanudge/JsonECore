using System.Text.Json;
using JsonECore.Context;

namespace JsonECore.Operators;

/// <summary>
/// Interface for JSON-E operators.
/// </summary>
public interface IOperator
{
    string Name { get; }
    JsonElement Execute(JsonElement template, EvaluationContext context, Func<JsonElement, EvaluationContext, JsonElement> render);
}
