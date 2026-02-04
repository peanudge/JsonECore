using System.Text.Json;
using JsonECore.Context;

namespace JsonECore.Operators;

/// <summary>
/// Registry for JSON-E operators.
/// </summary>
public class OperatorRegistry
{
    private static readonly Lazy<OperatorRegistry> _instance = new(() => new OperatorRegistry());
    public static OperatorRegistry Instance => _instance.Value;

    private readonly Dictionary<string, IOperator> _operators;

    private OperatorRegistry()
    {
        _operators = new Dictionary<string, IOperator>();
        RegisterDefaults();
    }

    private void RegisterDefaults()
    {
        Register(new EvalOperator());
        Register(new IfOperator());
        Register(new SwitchOperator());
        Register(new MatchOperator());
        Register(new LetOperator());
        Register(new MapOperator());
        Register(new ReduceOperator());
        Register(new FindOperator());
        Register(new FlattenOperator());
        Register(new FlattenDeepOperator());
        Register(new ReverseOperator());
        Register(new SortOperator());
        Register(new MergeOperator());
        Register(new MergeDeepOperator());
        Register(new JsonOperator());
        Register(new FromNowOperator());
    }

    public void Register(IOperator op)
    {
        _operators[op.Name] = op;
    }

    public bool TryGetOperator(string name, out IOperator? op)
    {
        return _operators.TryGetValue(name, out op);
    }

    public bool IsOperator(string name)
    {
        return _operators.ContainsKey(name);
    }

    public string? FindOperatorKey(JsonElement template)
    {
        if (template.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        foreach (var prop in template.EnumerateObject())
        {
            if (prop.Name.StartsWith("$") && _operators.ContainsKey(prop.Name))
            {
                return prop.Name;
            }
        }

        return null;
    }
}
