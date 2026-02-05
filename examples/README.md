# JsonECore Examples

Test templates and contexts for the JsonECore CLI.

## Running Examples

```bash
# From the project root directory:

# 01 - Basic Eval
dotnet run --project src/JsonECore.Cli -- eval -t examples/01-basic-eval.template.json -c examples/01-basic-eval.context.json

# 02 - Conditional Logic
dotnet run --project src/JsonECore.Cli -- eval -t examples/02-conditional.template.json -c examples/02-conditional.context.json

# 03 - Array Operations
dotnet run --project src/JsonECore.Cli -- eval -t examples/03-array-operations.template.json -c examples/03-array-operations.context.json

# 04 - Object Merge
dotnet run --project src/JsonECore.Cli -- eval -t examples/04-object-merge.template.json -c examples/04-object-merge.context.json

# 05 - Let Scope (Shopping Cart)
dotnet run --project src/JsonECore.Cli -- eval -t examples/05-let-scope.template.json -c examples/05-let-scope.context.json

# 06 - Built-in Functions
dotnet run --project src/JsonECore.Cli -- eval -t examples/06-functions.template.json -c examples/06-functions.context.json

# 07 - Flatten Nested Arrays
dotnet run --project src/JsonECore.Cli -- eval -t examples/07-flatten-nested.template.json -c examples/07-flatten-nested.context.json
```

## Example Descriptions

| # | Name | Features |
|---|------|----------|
| 01 | Basic Eval | `$eval`, arithmetic |
| 02 | Conditional | `$if`, `$switch`, interpolation |
| 03 | Array Operations | `$map`, `$reduce`, `$find`, `$sort`, `$reverse` |
| 04 | Object Merge | `$mergeDeep` |
| 05 | Let Scope | `$let`, nested scopes, shopping cart calculation |
| 06 | Functions | Math, string, type, range functions |
| 07 | Flatten | `$flatten`, `$flattenDeep` |
