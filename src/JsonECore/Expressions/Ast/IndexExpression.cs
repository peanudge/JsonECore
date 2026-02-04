using System.Text.Json;
using JsonECore.Context;

namespace JsonECore.Expressions.Ast;

/// <summary>
/// Represents an index access (e.g., arr[0], obj["key"]).
/// </summary>
public class IndexExpression : IExpression
{
    public IExpression Object { get; }
    public IExpression Index { get; }

    public IndexExpression(IExpression obj, IExpression index)
    {
        Object = obj;
        Index = index;
    }

    public JsonElement Evaluate(EvaluationContext context)
    {
        var obj = Object.Evaluate(context);
        var index = Index.Evaluate(context);

        if (obj.ValueKind == JsonValueKind.Array)
        {
            if (index.ValueKind != JsonValueKind.Number)
            {
                throw new JsonEException(JsonEErrorCodes.TypeMismatch, "Array index must be a number", "number", GetTypeName(index));
            }

            var idx = (int)index.GetDouble();
            var length = obj.GetArrayLength();

            // Support negative indexing
            if (idx < 0)
            {
                idx = length + idx;
            }

            if (idx < 0 || idx >= length)
            {
                throw new JsonEException(JsonEErrorCodes.IndexOutOfBounds, $"Index {idx} out of bounds for array of length {length}", idx);
            }

            return obj[idx].Clone();
        }

        if (obj.ValueKind == JsonValueKind.Object)
        {
            if (index.ValueKind != JsonValueKind.String)
            {
                throw new JsonEException(JsonEErrorCodes.TypeMismatch, "Object key must be a string", "string", GetTypeName(index));
            }

            var key = index.GetString()!;
            if (obj.TryGetProperty(key, out var value))
            {
                return value.Clone();
            }

            throw new JsonEException(JsonEErrorCodes.UndefinedVariable, $"Property '{key}' not found", key);
        }

        if (obj.ValueKind == JsonValueKind.String)
        {
            if (index.ValueKind != JsonValueKind.Number)
            {
                throw new JsonEException(JsonEErrorCodes.TypeMismatch, "String index must be a number", "number", GetTypeName(index));
            }

            var str = obj.GetString()!;
            var idx = (int)index.GetDouble();

            // Support negative indexing
            if (idx < 0)
            {
                idx = str.Length + idx;
            }

            if (idx < 0 || idx >= str.Length)
            {
                throw new JsonEException(JsonEErrorCodes.IndexOutOfBounds, $"Index {idx} out of bounds for string of length {str.Length}", idx);
            }

            return CreateString(str[idx].ToString());
        }

        throw new JsonEException(JsonEErrorCodes.TypeMismatch, "Cannot index into non-indexable value", "array/object/string", GetTypeName(obj));
    }

    private static string GetTypeName(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.Null => "null",
            JsonValueKind.True => "boolean",
            JsonValueKind.False => "boolean",
            JsonValueKind.Number => "number",
            JsonValueKind.String => "string",
            JsonValueKind.Array => "array",
            JsonValueKind.Object => "object",
            _ => "undefined"
        };
    }

    private static JsonElement CreateString(string value)
    {
        var json = JsonSerializer.Serialize(value);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }
}
