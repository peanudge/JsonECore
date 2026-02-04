using System.Text.Json;
using JsonECore.Context;

namespace JsonECore.Functions;

/// <summary>
/// Interface for built-in functions.
/// </summary>
public interface IBuiltInFunction
{
    string Name { get; }
    JsonElement Execute(List<JsonElement> args, EvaluationContext context);
}
