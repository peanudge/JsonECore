using System.Text.Json;
using JsonECore.Context;

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

        int? startIdx = Start != null ? (int)Start.Evaluate(context).GetDouble() : null;
        int? endIdx = End != null ? (int)End.Evaluate(context).GetDouble() : null;

        if (obj.ValueKind == JsonValueKind.Array)
        {
            return SliceArray(obj, startIdx, endIdx);
        }

        if (obj.ValueKind == JsonValueKind.String)
        {
            return SliceString(obj.GetString()!, startIdx, endIdx);
        }

        throw new JsonEException(JsonEErrorCodes.TypeMismatch, "Cannot slice non-sliceable value", "array/string", GetTypeName(obj));
    }

    private static JsonElement SliceArray(JsonElement array, int? start, int? end)
    {
        var length = array.GetArrayLength();
        var (startIdx, endIdx) = NormalizeSliceIndices(start, end, length);

        var result = new List<JsonElement>();
        for (int i = startIdx; i < endIdx; i++)
        {
            result.Add(array[i].Clone());
        }

        return CreateArray(result);
    }

    private static JsonElement SliceString(string str, int? start, int? end)
    {
        var length = str.Length;
        var (startIdx, endIdx) = NormalizeSliceIndices(start, end, length);

        if (startIdx >= endIdx)
        {
            return CreateString("");
        }

        return CreateString(str.Substring(startIdx, endIdx - startIdx));
    }

    private static (int start, int end) NormalizeSliceIndices(int? start, int? end, int length)
    {
        var startIdx = start ?? 0;
        var endIdx = end ?? length;

        // Handle negative indices
        if (startIdx < 0) startIdx = Math.Max(0, length + startIdx);
        if (endIdx < 0) endIdx = Math.Max(0, length + endIdx);

        // Clamp to valid range
        startIdx = Math.Min(startIdx, length);
        endIdx = Math.Min(endIdx, length);

        // Ensure start <= end
        if (startIdx > endIdx) startIdx = endIdx;

        return (startIdx, endIdx);
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

    private static JsonElement CreateArray(List<JsonElement> items)
    {
        var json = JsonSerializer.Serialize(items);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }
}
