# JsonECore Development Context

## Project Overview
JsonECore is a custom JSON-E template engine implementation for .NET 8.0, following the official JSON-E specification from https://github.com/json-e/json-e.

## Project Structure
```
JsonECore/
├── JsonECore.slnx              # Solution file
├── src/
│   ├── JsonECore/              # Main library (JsonECore.dll)
│   │   ├── JsonE.cs            # Main API entry point
│   │   ├── JsonEException.cs   # Custom exceptions with error codes
│   │   ├── DeleteMarker.cs     # Delete marker for conditional removal
│   │   ├── Context/
│   │   │   └── EvaluationContext.cs  # Variable scoping
│   │   ├── Operators/          # All 16 JSON-E operators
│   │   ├── Expressions/        # Expression parser & AST
│   │   │   ├── Tokenizer.cs
│   │   │   ├── ExpressionParser.cs
│   │   │   ├── ExpressionEvaluator.cs
│   │   │   └── Ast/            # AST node types
│   │   ├── Functions/          # Built-in functions
│   │   └── Localization/       # i18n infrastructure
│   └── JsonECore.Cli/          # CLI tool (jsone command)
└── tests/
    └── JsonECore.Tests/        # xUnit tests (101 tests)
```

## Build & Test Commands
```bash
# Build
dotnet build

# Run tests
dotnet test

# Run CLI
dotnet run --project src/JsonECore.Cli -- eval -e '{"$eval": "1 + 2"}'
dotnet run --project src/JsonECore.Cli -- eval -e '{"$if": "x > 5", "then": "big", "else": "small"}' -x '{"x": 10}'
```

## Implemented Features

### Operators (16)
| Operator | Description | Example |
|----------|-------------|---------|
| `$eval` | Evaluate expression | `{"$eval": "1 + 2"}` |
| `$if` | Conditional | `{"$if": "cond", "then": a, "else": b}` |
| `$switch` | Switch-case | `{"$switch": {"x == 1": "yes", "$default": "no"}}` |
| `$match` | Return all matches | `{"$match": {"x > 5": "big", "x < 5": "small"}}` |
| `$let` | Variable binding | `{"$let": {"x": 10}, "in": {"$eval": "x + 5"}}` |
| `$map` | Iteration | `{"$map": [1,2,3], "each(x)": {"$eval": "x * 2"}}` |
| `$reduce` | Accumulation | `{"$reduce": arr, "each(acc, x)": expr, "initial": 0}` |
| `$find` | Find first match | `{"$find": [1,2,3], "each(x)": "x == 2"}` |
| `$flatten` | Single-level flatten | `{"$flatten": [[1,2], [3,4]]}` |
| `$flattenDeep` | Deep flatten | `{"$flattenDeep": [[[1]], [[2]]]}` |
| `$reverse` | Reverse array/string | `{"$reverse": [3,2,1]}` |
| `$sort` | Sort array | `{"$sort": arr, "by(x)": "x.name"}` |
| `$merge` | Merge objects | `{"$merge": [{a: 1}, {b: 2}]}` |
| `$mergeDeep` | Deep merge | `{"$mergeDeep": [{a: {b: 1}}, {a: {c: 2}}]}` |
| `$json` | JSON stringify | `{"$json": {a: 1}}` |
| `$fromNow` | Date calculation | `{"$fromNow": "2 days"}` |

### Expression Language
- **Arithmetic**: `+`, `-`, `*`, `/`, `%`, `**`
- **Comparison**: `==`, `!=`, `<`, `<=`, `>`, `>=`
- **Logical**: `&&`, `||`, `!`
- **Membership**: `in`
- **Ternary**: `a ? b : c`
- **Array access**: `arr[0]`, `arr[-1]` (negative indexing)
- **Slicing**: `arr[1:3]`, `arr[:-1]`
- **Property access**: `obj.property`
- **String interpolation**: `"Hello ${name}!"`

### Built-in Functions (15)
- **Math**: `min`, `max`, `sqrt`, `ceil`, `floor`, `abs`
- **String**: `lowercase`, `uppercase`, `strip`, `lstrip`, `rstrip`, `split`, `join`
- **Type**: `typeof`, `str`, `number`, `len`
- **Utility**: `range`, `defined`, `fromNow`

## API Usage
```csharp
using JsonECore;
using System.Text.Json;

// String API
var result = JsonE.Render(
    """{"$if": "x > 5", "then": "big", "else": "small"}""",
    """{"x": 10}"""
);
// Result: "big"

// JsonElement API
var template = JsonDocument.Parse("""{"message": "Hello ${name}!"}""").RootElement;
var context = JsonDocument.Parse("""{"name": "World"}""").RootElement;
var result = JsonE.Render(template, context);
// Result: {"message": "Hello World!"}
```

## Key Design Decisions
- Uses `System.Text.Json` (native .NET, no external dependencies)
- Hand-written recursive descent parser for expressions
- Delete markers for `$if` without else branch (removes keys/elements)
- i18n ready with LocalizationManager and error codes

## Error Codes
- `JSONE001`: Invalid operator
- `JSONE002`: Syntax error
- `JSONE003`: Undefined variable
- `JSONE004`: Type mismatch
- `JSONE005`: Division by zero
- `JSONE006`: Index out of bounds
- `JSONE007`: Invalid function call
- `JSONE008`: Invalid date/time expression
- `JSONE009`: Invalid template
- `JSONE010`: Invalid argument

## Reference
- JSON-E Specification: https://github.com/json-e/json-e/blob/main/specification.yml
- JSON-E Documentation: https://json-e.js.org/
