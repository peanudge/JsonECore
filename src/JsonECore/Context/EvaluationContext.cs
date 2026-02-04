using System.Text.Json;

namespace JsonECore.Context;

/// <summary>
/// Manages variable scoping during JSON-E template evaluation.
/// </summary>
public class EvaluationContext
{
    private readonly Dictionary<string, JsonElement> _variables;
    private readonly EvaluationContext? _parent;
    private readonly JsonSerializerOptions _serializerOptions;

    public EvaluationContext(JsonElement context)
    {
        _variables = new Dictionary<string, JsonElement>();
        _parent = null;
        _serializerOptions = new JsonSerializerOptions();

        if (context.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in context.EnumerateObject())
            {
                _variables[property.Name] = property.Value.Clone();
            }
        }
    }

    public EvaluationContext(EvaluationContext parent, Dictionary<string, JsonElement>? additionalVariables = null)
    {
        _parent = parent;
        _variables = additionalVariables ?? new Dictionary<string, JsonElement>();
        _serializerOptions = parent._serializerOptions;
    }

    public bool TryGetVariable(string name, out JsonElement value)
    {
        if (_variables.TryGetValue(name, out value))
        {
            return true;
        }

        if (_parent != null)
        {
            return _parent.TryGetVariable(name, out value);
        }

        value = default;
        return false;
    }

    public JsonElement GetVariable(string name)
    {
        if (TryGetVariable(name, out var value))
        {
            return value;
        }

        throw new JsonEException(
            JsonEErrorCodes.UndefinedVariable,
            $"Undefined variable: {name}",
            name);
    }

    public bool HasVariable(string name)
    {
        return TryGetVariable(name, out _);
    }

    public void SetVariable(string name, JsonElement value)
    {
        _variables[name] = value.Clone();
    }

    public void SetVariable(string name, object? value)
    {
        var json = JsonSerializer.Serialize(value, _serializerOptions);
        using var doc = JsonDocument.Parse(json);
        _variables[name] = doc.RootElement.Clone();
    }

    public EvaluationContext CreateChildContext(Dictionary<string, JsonElement>? additionalVariables = null)
    {
        return new EvaluationContext(this, additionalVariables);
    }

    public EvaluationContext CreateChildContext(string variableName, JsonElement value)
    {
        var vars = new Dictionary<string, JsonElement> { { variableName, value.Clone() } };
        return new EvaluationContext(this, vars);
    }

    public Dictionary<string, JsonElement> GetAllVariables()
    {
        var result = new Dictionary<string, JsonElement>();

        if (_parent != null)
        {
            foreach (var kvp in _parent.GetAllVariables())
            {
                result[kvp.Key] = kvp.Value;
            }
        }

        foreach (var kvp in _variables)
        {
            result[kvp.Key] = kvp.Value;
        }

        return result;
    }
}
