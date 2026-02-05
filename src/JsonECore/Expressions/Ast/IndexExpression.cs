using System.Text.Json;
using JsonECore.Context;
using static JsonECore.JsonElementHelper;

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

        if (IsArray(obj))
        {
            var idx = GetInt(index, "array index");
            var length = obj.GetArrayLength();

            if (idx < 0) idx = length + idx;
            if (idx < 0 || idx >= length)
                throw new JsonEException(JsonEErrorCodes.IndexOutOfBounds, $"Index {idx} out of bounds for array of length {length}", idx);

            return obj[idx].Clone();
        }

        if (IsObject(obj))
        {
            var key = GetString(index, "object key");
            if (obj.TryGetProperty(key, out var value))
                return value.Clone();
            throw new JsonEException(JsonEErrorCodes.UndefinedVariable, $"Property '{key}' not found", key);
        }

        if (IsString(obj))
        {
            var str = obj.GetString()!;
            var idx = GetInt(index, "string index");

            if (idx < 0) idx = str.Length + idx;
            if (idx < 0 || idx >= str.Length)
                throw new JsonEException(JsonEErrorCodes.IndexOutOfBounds, $"Index {idx} out of bounds for string of length {str.Length}", idx);

            return CreateString(str[idx].ToString());
        }

        throw new JsonEException(JsonEErrorCodes.TypeMismatch, "Cannot index into non-indexable value", "array/object/string", GetTypeName(obj));
    }
}
