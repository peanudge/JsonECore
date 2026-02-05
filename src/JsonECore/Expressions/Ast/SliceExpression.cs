using System.Text.Json;
using JsonECore.Context;
using static JsonECore.JsonElementHelper;

namespace JsonECore.Expressions.Ast;

/// <summary>
/// Represents a slice operation (e.g., arr[1:3], arr[:-1]).
/// </summary>
public class SliceExpression : IExpression
{
    public IExpression Object { get; }
    public IExpression? Start { get; }
    public IExpression? End { get; }

    public SliceExpression(IExpression obj, IExpression? start, IExpression? end)
    {
        Object = obj;
        Start = start;
        End = end;
    }

    public JsonElement Evaluate(EvaluationContext context)
    {
        var obj = Object.Evaluate(context);

        int? startIdx = Start != null ? GetInt(Start.Evaluate(context)) : null;
        int? endIdx = End != null ? GetInt(End.Evaluate(context)) : null;

        if (IsArray(obj))
            return SliceArray(obj, startIdx, endIdx);

        if (IsString(obj))
            return SliceString(obj.GetString()!, startIdx, endIdx);

        throw new JsonEException(JsonEErrorCodes.TypeMismatch, "Cannot slice non-sliceable value", "array/string", GetTypeName(obj));
    }

    private static JsonElement SliceArray(JsonElement array, int? start, int? end)
    {
        var length = array.GetArrayLength();
        var (startIdx, endIdx) = NormalizeSliceIndices(start, end, length);

        var result = new List<JsonElement>();
        for (int i = startIdx; i < endIdx; i++)
            result.Add(array[i].Clone());

        return CreateArray(result);
    }

    private static JsonElement SliceString(string str, int? start, int? end)
    {
        var (startIdx, endIdx) = NormalizeSliceIndices(start, end, str.Length);
        if (startIdx >= endIdx) return CreateString("");
        return CreateString(str.Substring(startIdx, endIdx - startIdx));
    }

    private static (int start, int end) NormalizeSliceIndices(int? start, int? end, int length)
    {
        var startIdx = start ?? 0;
        var endIdx = end ?? length;

        if (startIdx < 0) startIdx = Math.Max(0, length + startIdx);
        if (endIdx < 0) endIdx = Math.Max(0, length + endIdx);

        startIdx = Math.Min(startIdx, length);
        endIdx = Math.Min(endIdx, length);
        if (startIdx > endIdx) startIdx = endIdx;

        return (startIdx, endIdx);
    }
}
