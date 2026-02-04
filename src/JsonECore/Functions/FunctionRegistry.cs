using System.Text.Json;
using JsonECore.Context;

namespace JsonECore.Functions;

/// <summary>
/// Registry for built-in functions.
/// </summary>
public class FunctionRegistry
{
    private static readonly Lazy<FunctionRegistry> _instance = new(() => new FunctionRegistry());
    public static FunctionRegistry Instance => _instance.Value;

    private readonly Dictionary<string, IBuiltInFunction> _functions;

    private FunctionRegistry()
    {
        _functions = new Dictionary<string, IBuiltInFunction>();
        RegisterDefaults();
    }

    private void RegisterDefaults()
    {
        // Math functions
        Register(new MathFunctions.MinFunction());
        Register(new MathFunctions.MaxFunction());
        Register(new MathFunctions.SqrtFunction());
        Register(new MathFunctions.CeilFunction());
        Register(new MathFunctions.FloorFunction());
        Register(new MathFunctions.AbsFunction());

        // String functions
        Register(new StringFunctions.LowercaseFunction());
        Register(new StringFunctions.UppercaseFunction());
        Register(new StringFunctions.StripFunction());
        Register(new StringFunctions.LstripFunction());
        Register(new StringFunctions.RstripFunction());
        Register(new StringFunctions.SplitFunction());
        Register(new StringFunctions.JoinFunction());

        // Type functions
        Register(new TypeFunctions.TypeofFunction());
        Register(new TypeFunctions.StrFunction());
        Register(new TypeFunctions.NumberFunction());
        Register(new TypeFunctions.LenFunction());

        // Utility functions
        Register(new UtilityFunctions.RangeFunction());
        Register(new UtilityFunctions.DefinedFunction());
        Register(new UtilityFunctions.FromNowFunction());
    }

    public void Register(IBuiltInFunction function)
    {
        _functions[function.Name] = function;
    }

    public bool HasFunction(string name)
    {
        return _functions.ContainsKey(name);
    }

    public JsonElement Call(string name, List<JsonElement> args, EvaluationContext context)
    {
        if (!_functions.TryGetValue(name, out var function))
        {
            throw new JsonEException(JsonEErrorCodes.InvalidFunctionCall, $"Unknown function: {name}", name);
        }

        return function.Execute(args, context);
    }
}
