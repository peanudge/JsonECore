# Creating Custom Operators in JsonECore

This guide explains how to create and register custom operators in JsonECore.

## Overview

Operators are special JSON-E constructs that begin with `$` (e.g., `$if`, `$map`, `$eval`). You can extend JsonECore by creating your own custom operators.

## IOperator Interface

All operators must implement the `IOperator` interface:

```csharp
public interface IOperator
{
    string Name { get; }
    JsonElement Execute(
        JsonElement template,
        EvaluationContext context,
        Func<JsonElement, EvaluationContext, JsonElement> render
    );
}
```

| Property/Method | Description |
|-----------------|-------------|
| `Name` | The operator name including `$` prefix (e.g., `"$random"`) |
| `Execute` | Evaluates the operator and returns the result |

### Execute Parameters

| Parameter | Description |
|-----------|-------------|
| `template` | The full JSON object containing the operator |
| `context` | Current evaluation context with variables |
| `render` | Function to recursively render nested templates |

## Step-by-Step Guide

### Step 1: Create the Operator Class

Create a new file in `src/JsonECore/Operators/`:

```csharp
using System.Text.Json;
using JsonECore.Context;
using static JsonECore.JsonElementHelper;

namespace JsonECore.Operators;

/// <summary>
/// Implements the $random operator.
/// Usage: {"$random": {"min": 0, "max": 100}}
/// </summary>
public class RandomOperator : IOperator
{
    public string Name => "$random";

    public JsonElement Execute(
        JsonElement template,
        EvaluationContext context,
        Func<JsonElement, EvaluationContext, JsonElement> render)
    {
        // 1. Extract operator value
        if (!template.TryGetProperty("$random", out var config))
        {
            throw new JsonEException(
                JsonEErrorCodes.InvalidTemplate,
                "$random requires configuration",
                Name);
        }

        // 2. Render nested expressions (important!)
        var configValue = render(config, context);

        // 3. Parse parameters with defaults
        int min = 0, max = 100;

        if (configValue.ValueKind == JsonValueKind.Object)
        {
            if (configValue.TryGetProperty("min", out var minProp))
                min = GetInt(minProp, "$random min");

            if (configValue.TryGetProperty("max", out var maxProp))
                max = GetInt(maxProp, "$random max");
        }
        else if (configValue.ValueKind == JsonValueKind.Number)
        {
            // Support shorthand: {"$random": 100} means 0-100
            max = GetInt(configValue, "$random");
        }

        // 4. Execute logic
        var random = new Random();
        var result = random.Next(min, max + 1);

        // 5. Return result as JsonElement
        return CreateNumber(result);
    }
}
```

### Step 2: Register the Operator

Add your operator to `OperatorRegistry.cs`:

```csharp
private void RegisterDefaults()
{
    // ... existing operators ...
    Register(new RandomOperator());  // Add this line
}
```

### Step 3: Use the Operator

```json
// Full syntax
{"$random": {"min": 1, "max": 6}}

// Shorthand (0 to max)
{"$random": 100}

// With expressions
{"$random": {"min": 0, "max": {"$eval": "maxValue"}}}
```

## JsonElementHelper Utilities

Use `JsonElementHelper` for common operations:

```csharp
using static JsonECore.JsonElementHelper;
```

### Type Checking
```csharp
IsNumber(value)      // true if number
IsString(value)      // true if string
IsArray(value)       // true if array
IsObject(value)      // true if object
IsBoolean(value)     // true if boolean
IsNull(value)        // true if null
GetTypeName(value)   // "number", "string", "array", etc.
IsTruthy(value)      // JSON-E truthiness check
```

### Value Extraction
```csharp
GetNumber(value, "context")   // Extract double, throws on type mismatch
GetInt(value, "context")      // Extract int
GetString(value, "context")   // Extract string
GetBoolean(value)             // Extract bool
```

### Value Creation
```csharp
CreateNull()                  // JSON null
CreateBool(true)              // JSON true/false
CreateNumber(42)              // JSON number
CreateNumber(3.14)            // JSON number (double)
CreateString("hello")         // JSON string
CreateArray(elements)         // JSON array from IEnumerable<JsonElement>
CreateArray(ints)             // JSON array from IEnumerable<int>
CreateArray(strings)          // JSON array from IEnumerable<string>
CreateObject(dict)            // JSON object from Dictionary<string, JsonElement>
CreateEmptyObject()           // {}
CreateEmptyArray()            // []
```

### Conversion
```csharp
ConvertToString(value)        // Convert any value to string representation
FormatNumber(3.0)             // "3" (no decimal for integers)
```

### Comparison
```csharp
AreEqual(left, right)         // Deep equality check
Compare(left, right)          // -1, 0, 1 comparison
```

## Error Handling

Use `JsonEException` with appropriate error codes:

```csharp
throw new JsonEException(
    JsonEErrorCodes.InvalidTemplate,  // Error code
    "Error message",                   // Human-readable message
    Name                               // Operator name for context
);

throw new JsonEException(
    JsonEErrorCodes.TypeMismatch,
    "Expected number",
    "number",                          // Expected type
    GetTypeName(value)                 // Actual type
);
```

### Error Codes

| Code | Constant | Description |
|------|----------|-------------|
| JSONE001 | `InvalidOperator` | Unknown operator |
| JSONE002 | `SyntaxError` | Expression syntax error |
| JSONE003 | `UndefinedVariable` | Variable not found |
| JSONE004 | `TypeMismatch` | Type mismatch |
| JSONE005 | `DivisionByZero` | Division by zero |
| JSONE006 | `IndexOutOfBounds` | Array index out of bounds |
| JSONE007 | `InvalidFunctionCall` | Invalid function call |
| JSONE008 | `InvalidDateTime` | Invalid date/time |
| JSONE009 | `InvalidTemplate` | Invalid template structure |
| JSONE010 | `InvalidArgument` | Invalid argument |

## Advanced Patterns

### Using Child Contexts

Create scoped variables for iteration:

```csharp
// Single variable
var childContext = context.CreateChildContext("item", itemValue);

// Multiple variables
var vars = new Dictionary<string, JsonElement>
{
    { "item", itemValue },
    { "index", CreateNumber(i) }
};
var childContext = context.CreateChildContext(vars);
```

### Parsing `each(x)` or `by(x)` Patterns

For operators with variable binding syntax:

```csharp
private static readonly Regex EachRegex =
    new(@"^each\((\w+)(?:,\s*(\w+))?\)$", RegexOptions.Compiled);

// Find property like "each(item)" or "each(item, index)"
foreach (var prop in template.EnumerateObject())
{
    if (prop.Name.StartsWith("each("))
    {
        var match = EachRegex.Match(prop.Name);
        if (match.Success)
        {
            string itemVar = match.Groups[1].Value;
            string? indexVar = match.Groups[2].Success
                ? match.Groups[2].Value
                : null;
            JsonElement body = prop.Value;
            // ... use variables
        }
    }
}
```

### Returning Delete Markers

For conditional removal (like `$if` without `else`):

```csharp
return DeleteMarker.Create();
```

## Complete Example: $uuid Operator

```csharp
using System.Text.Json;
using JsonECore.Context;
using static JsonECore.JsonElementHelper;

namespace JsonECore.Operators;

/// <summary>
/// Generates a UUID/GUID.
/// Usage: {"$uuid": true} or {"$uuid": "v4"}
/// </summary>
public class UuidOperator : IOperator
{
    public string Name => "$uuid";

    public JsonElement Execute(
        JsonElement template,
        EvaluationContext context,
        Func<JsonElement, EvaluationContext, JsonElement> render)
    {
        if (!template.TryGetProperty("$uuid", out var config))
        {
            throw new JsonEException(
                JsonEErrorCodes.InvalidTemplate,
                "$uuid requires a value",
                Name);
        }

        var configValue = render(config, context);

        string format = "D"; // Default: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx

        if (configValue.ValueKind == JsonValueKind.String)
        {
            var formatStr = configValue.GetString();
            format = formatStr switch
            {
                "N" or "n" => "N",  // No hyphens
                "B" or "b" => "B",  // Braces
                "P" or "p" => "P",  // Parentheses
                _ => "D"            // Default with hyphens
            };
        }

        return CreateString(Guid.NewGuid().ToString(format));
    }
}
```

## Testing Custom Operators

Create tests in `tests/JsonECore.Tests/OperatorTests/`:

```csharp
public class RandomOperatorTests
{
    [Fact]
    public void Random_ReturnsNumberInRange()
    {
        var template = """{"$random": {"min": 1, "max": 10}}""";
        var result = JsonE.Render(template, "{}");
        var value = int.Parse(result);

        Assert.InRange(value, 1, 10);
    }

    [Fact]
    public void Random_SupportsExpressions()
    {
        var template = """{"$random": {"min": 0, "max": {"$eval": "maxVal"}}}""";
        var context = """{"maxVal": 5}""";
        var result = JsonE.Render(template, context);
        var value = int.Parse(result);

        Assert.InRange(value, 0, 5);
    }

    [Fact]
    public void Random_ThrowsOnInvalidType()
    {
        var template = """{"$random": {"min": "invalid"}}""";

        Assert.Throws<JsonEException>(() => JsonE.Render(template, "{}"));
    }
}
```

## Summary

1. Implement `IOperator` interface
2. Use `JsonElementHelper` for type-safe operations
3. Always call `render()` on nested values to support expressions
4. Throw `JsonEException` with appropriate error codes
5. Register in `OperatorRegistry.RegisterDefaults()`
6. Write tests for your operator
