using System.Text.Json;

namespace JsonECore;

/// <summary>
/// Shared helper methods for JsonElement operations.
/// Eliminates duplication across operators, functions, and expressions.
/// </summary>
public static class JsonElementHelper
{
    #region Type Checking

    public static string GetTypeName(JsonElement value)
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

    public static bool IsTruthy(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => false,
            JsonValueKind.Undefined => false,
            JsonValueKind.Number => value.GetDouble() != 0,
            JsonValueKind.String => !string.IsNullOrEmpty(value.GetString()),
            JsonValueKind.Array => value.GetArrayLength() > 0,
            JsonValueKind.Object => true,
            _ => false
        };
    }

    public static bool IsNumber(JsonElement value) => value.ValueKind == JsonValueKind.Number;
    public static bool IsString(JsonElement value) => value.ValueKind == JsonValueKind.String;
    public static bool IsArray(JsonElement value) => value.ValueKind == JsonValueKind.Array;
    public static bool IsObject(JsonElement value) => value.ValueKind == JsonValueKind.Object;
    public static bool IsBoolean(JsonElement value) => value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False;
    public static bool IsNull(JsonElement value) => value.ValueKind == JsonValueKind.Null;

    #endregion

    #region Value Extraction

    public static double GetNumber(JsonElement value, string context = "")
    {
        if (value.ValueKind != JsonValueKind.Number)
        {
            var msg = string.IsNullOrEmpty(context)
                ? $"Expected number, got {GetTypeName(value)}"
                : $"{context}: expected number, got {GetTypeName(value)}";
            throw new JsonEException(JsonEErrorCodes.TypeMismatch, msg, "number", GetTypeName(value));
        }
        return value.GetDouble();
    }

    public static int GetInt(JsonElement value, string context = "")
    {
        return (int)GetNumber(value, context);
    }

    public static string GetString(JsonElement value, string context = "")
    {
        if (value.ValueKind != JsonValueKind.String)
        {
            var msg = string.IsNullOrEmpty(context)
                ? $"Expected string, got {GetTypeName(value)}"
                : $"{context}: expected string, got {GetTypeName(value)}";
            throw new JsonEException(JsonEErrorCodes.TypeMismatch, msg, "string", GetTypeName(value));
        }
        return value.GetString()!;
    }

    public static bool GetBoolean(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => throw new JsonEException(JsonEErrorCodes.TypeMismatch, $"Expected boolean, got {GetTypeName(value)}", "boolean", GetTypeName(value))
        };
    }

    #endregion

    #region Value Creation

    public static JsonElement CreateNull()
    {
        using var doc = JsonDocument.Parse("null");
        return doc.RootElement.Clone();
    }

    public static JsonElement CreateBool(bool value)
    {
        using var doc = JsonDocument.Parse(value ? "true" : "false");
        return doc.RootElement.Clone();
    }

    public static JsonElement CreateNumber(double value)
    {
        var json = JsonSerializer.Serialize(value);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    public static JsonElement CreateNumber(int value)
    {
        var json = JsonSerializer.Serialize(value);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    public static JsonElement CreateString(string value)
    {
        var json = JsonSerializer.Serialize(value);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    public static JsonElement CreateArray(IEnumerable<JsonElement> items)
    {
        var json = JsonSerializer.Serialize(items);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    public static JsonElement CreateArray(IEnumerable<int> items)
    {
        var json = JsonSerializer.Serialize(items);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    public static JsonElement CreateArray(IEnumerable<string> items)
    {
        var json = JsonSerializer.Serialize(items);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    public static JsonElement CreateObject(Dictionary<string, JsonElement> properties)
    {
        var json = JsonSerializer.Serialize(properties);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    public static JsonElement CreateEmptyObject()
    {
        using var doc = JsonDocument.Parse("{}");
        return doc.RootElement.Clone();
    }

    public static JsonElement CreateEmptyArray()
    {
        using var doc = JsonDocument.Parse("[]");
        return doc.RootElement.Clone();
    }

    public static JsonElement FromValue(object? value)
    {
        var json = JsonSerializer.Serialize(value);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    #endregion

    #region Conversion

    public static string ConvertToString(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString() ?? "",
            JsonValueKind.Number => FormatNumber(value.GetDouble()),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => "null",
            _ => value.GetRawText()
        };
    }

    public static string FormatNumber(double value)
    {
        // Format integers without decimal point
        if (value == Math.Truncate(value) && !double.IsInfinity(value) && Math.Abs(value) < long.MaxValue)
        {
            return ((long)value).ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
        return value.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    #endregion

    #region Comparison

    public static bool AreEqual(JsonElement left, JsonElement right)
    {
        if (left.ValueKind != right.ValueKind)
        {
            // Handle number comparisons across types
            if (IsNumber(left) && IsNumber(right))
            {
                return left.GetDouble() == right.GetDouble();
            }
            return false;
        }

        return left.ValueKind switch
        {
            JsonValueKind.Null => true,
            JsonValueKind.True => true,
            JsonValueKind.False => true,
            JsonValueKind.Number => left.GetDouble() == right.GetDouble(),
            JsonValueKind.String => left.GetString() == right.GetString(),
            JsonValueKind.Array => ArraysEqual(left, right),
            JsonValueKind.Object => ObjectsEqual(left, right),
            _ => false
        };
    }

    private static bool ArraysEqual(JsonElement left, JsonElement right)
    {
        if (left.GetArrayLength() != right.GetArrayLength())
            return false;

        var leftArr = left.EnumerateArray().ToList();
        var rightArr = right.EnumerateArray().ToList();

        for (int i = 0; i < leftArr.Count; i++)
        {
            if (!AreEqual(leftArr[i], rightArr[i]))
                return false;
        }
        return true;
    }

    private static bool ObjectsEqual(JsonElement left, JsonElement right)
    {
        var leftProps = left.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);
        var rightProps = right.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);

        if (leftProps.Count != rightProps.Count)
            return false;

        foreach (var kvp in leftProps)
        {
            if (!rightProps.TryGetValue(kvp.Key, out var rightVal))
                return false;
            if (!AreEqual(kvp.Value, rightVal))
                return false;
        }
        return true;
    }

    public static int Compare(JsonElement left, JsonElement right)
    {
        if (IsNumber(left) && IsNumber(right))
        {
            return left.GetDouble().CompareTo(right.GetDouble());
        }

        if (IsString(left) && IsString(right))
        {
            return string.Compare(left.GetString(), right.GetString(), StringComparison.Ordinal);
        }

        throw new JsonEException(JsonEErrorCodes.TypeMismatch,
            $"Cannot compare {GetTypeName(left)} with {GetTypeName(right)}",
            GetTypeName(left), GetTypeName(right));
    }

    #endregion
}
