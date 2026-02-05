# JsonECore

A custom JSON-E template engine implementation for .NET 8.0, providing a powerful way to transform JSON data using a declarative template language.

## Features

- **16 Operators**: `$eval`, `$if`, `$switch`, `$match`, `$let`, `$map`, `$reduce`, `$find`, `$flatten`, `$flattenDeep`, `$reverse`, `$sort`, `$merge`, `$mergeDeep`, `$json`, `$fromNow`
- **Rich Expression Language**: Arithmetic, comparison, logical operators, array slicing, property access
- **15 Built-in Functions**: Math, string manipulation, type conversion, utilities
- **String Interpolation**: `${expression}` syntax support
- **CLI Tool**: Command-line interface for template evaluation and validation
- **i18n Ready**: Internationalization infrastructure for error messages

## Installation

```bash
# Clone the repository
git clone https://github.com/your-repo/JsonECore.git
cd JsonECore

# Build
dotnet build

# Run tests
dotnet test
```

## Quick Start

### Library Usage

```csharp
using JsonECore;

// Simple expression evaluation
var result = JsonE.Render(
    """{"$eval": "1 + 2"}""",
    "{}"
);
// Result: "3"

// Conditional logic
var result = JsonE.Render(
    """{"$if": "user.isAdmin", "then": "Welcome, Admin!", "else": "Welcome!"}""",
    """{"user": {"isAdmin": true}}"""
);
// Result: "Welcome, Admin!"

// Array transformation
var result = JsonE.Render(
    """{"$map": [1, 2, 3], "each(x)": {"$eval": "x * 2"}}""",
    "{}"
);
// Result: [2, 4, 6]

// String interpolation
var result = JsonE.Render(
    """{"message": "Hello, ${name}!"}""",
    """{"name": "World"}"""
);
// Result: {"message": "Hello, World!"}
```

### CLI Usage

```bash
# Evaluate a template
dotnet run --project src/JsonECore.Cli -- eval -e '{"$eval": "1 + 2"}'
# Output: 3

# Evaluate with context
dotnet run --project src/JsonECore.Cli -- eval \
  -e '{"$if": "x > 5", "then": "big", "else": "small"}' \
  -x '{"x": 10}'
# Output: "big"

# Validate a template
dotnet run --project src/JsonECore.Cli -- validate -e '{"$eval": "1 + 2"}'
# Output: Template is valid.

# Use files
dotnet run --project src/JsonECore.Cli -- eval -t template.json -c context.json
```

## Operators

| Operator | Description | Example |
|----------|-------------|---------|
| `$eval` | Evaluate expression | `{"$eval": "x + y"}` |
| `$if` | Conditional | `{"$if": "cond", "then": a, "else": b}` |
| `$switch` | Multi-way conditional | `{"$switch": {"x == 1": "one", "$default": "other"}}` |
| `$match` | Return all matches | `{"$match": {"x > 0": "positive", "x < 10": "small"}}` |
| `$let` | Variable binding | `{"$let": {"x": 10}, "in": {"$eval": "x * 2"}}` |
| `$map` | Transform array | `{"$map": [1,2,3], "each(x)": {"$eval": "x * 2"}}` |
| `$reduce` | Accumulate values | `{"$reduce": [1,2,3], "each(acc,x)": {"$eval": "acc+x"}, "initial": 0}` |
| `$find` | Find first match | `{"$find": [1,2,3], "each(x)": "x > 1"}` |
| `$flatten` | Flatten one level | `{"$flatten": [[1,2], [3,4]]}` |
| `$flattenDeep` | Flatten recursively | `{"$flattenDeep": [[[1]], [[2]]]}` |
| `$reverse` | Reverse array/string | `{"$reverse": [1,2,3]}` |
| `$sort` | Sort array | `{"$sort": [3,1,2]}` or `{"$sort": arr, "by(x)": "x.name"}` |
| `$merge` | Merge objects | `{"$merge": [{"a": 1}, {"b": 2}]}` |
| `$mergeDeep` | Deep merge objects | `{"$mergeDeep": [{"a": {"b": 1}}, {"a": {"c": 2}}]}` |
| `$json` | Convert to JSON string | `{"$json": {"a": 1}}` |
| `$fromNow` | Date/time calculation | `{"$fromNow": "2 days"}` |

## Expression Language

### Operators
- **Arithmetic**: `+`, `-`, `*`, `/`, `%`, `**` (power)
- **Comparison**: `==`, `!=`, `<`, `<=`, `>`, `>=`
- **Logical**: `&&`, `||`, `!`
- **Membership**: `in`
- **Ternary**: `condition ? true_value : false_value`

### Access Patterns
```javascript
array[0]        // Index access
array[-1]       // Negative index (from end)
array[1:3]      // Slice
object.property // Property access
object["key"]   // Bracket notation
```

## Built-in Functions

| Category | Functions |
|----------|-----------|
| Math | `min(a, b, ...)`, `max(a, b, ...)`, `sqrt(x)`, `ceil(x)`, `floor(x)`, `abs(x)` |
| String | `lowercase(s)`, `uppercase(s)`, `strip(s)`, `lstrip(s)`, `rstrip(s)`, `split(s, delim)`, `join(arr, sep)` |
| Type | `typeof(x)`, `str(x)`, `number(x)`, `len(x)` |
| Utility | `range(end)`, `range(start, end)`, `range(start, end, step)`, `defined(name)`, `fromNow(duration)` |

## Project Structure

```
JsonECore/
├── src/
│   ├── JsonECore/           # Main library
│   │   ├── JsonE.cs         # Public API
│   │   ├── Context/         # Variable scoping
│   │   ├── Operators/       # Operator implementations
│   │   ├── Expressions/     # Expression parser
│   │   ├── Functions/       # Built-in functions
│   │   └── Localization/    # i18n support
│   └── JsonECore.Cli/       # CLI tool
└── tests/
    └── JsonECore.Tests/     # Unit tests (101 tests)
```

## Requirements

- .NET 8.0 SDK

## Reference

Based on the [JSON-E specification](https://github.com/json-e/json-e).

## License

MIT
