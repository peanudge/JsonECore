using System.Text.Json;

namespace JsonECore;

/// <summary>
/// Represents a delete marker in JSON-E.
/// When a delete marker appears as a value in an object, the key is removed.
/// When it appears in an array, the element is removed.
/// </summary>
public static class DeleteMarker
{
    // We use a special unique string as a marker
    private const string MarkerValue = "__JSONE_DELETE_MARKER__";

    public static JsonElement Create()
    {
        var json = JsonSerializer.Serialize(MarkerValue);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    public static bool IsDeleteMarker(JsonElement element)
    {
        return element.ValueKind == JsonValueKind.String &&
               element.GetString() == MarkerValue;
    }
}
