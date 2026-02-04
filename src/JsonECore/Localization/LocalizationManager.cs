using System.Globalization;

namespace JsonECore.Localization;

/// <summary>
/// Manages localized error messages for JSON-E.
/// </summary>
public class LocalizationManager : ILocalizationProvider
{
    private static readonly Lazy<LocalizationManager> _instance = new(() => new LocalizationManager());
    public static LocalizationManager Instance => _instance.Value;

    private readonly Dictionary<string, Dictionary<string, string>> _messages;
    private CultureInfo _currentCulture;

    private LocalizationManager()
    {
        _currentCulture = CultureInfo.CurrentUICulture;
        _messages = new Dictionary<string, Dictionary<string, string>>
        {
            ["en"] = new Dictionary<string, string>
            {
                ["JSONE001"] = "Invalid operator \"{0}\"",
                ["JSONE002"] = "Syntax error at position {0}: {1}",
                ["JSONE003"] = "Undefined variable \"{0}\"",
                ["JSONE004"] = "Type mismatch: expected {0}, got {1}",
                ["JSONE005"] = "Division by zero",
                ["JSONE006"] = "Index out of bounds: {0}",
                ["JSONE007"] = "Invalid function call: {0}",
                ["JSONE008"] = "Invalid date/time expression: {0}",
                ["JSONE009"] = "Invalid template: {0}",
                ["JSONE010"] = "Invalid argument: {0}"
            }
        };
    }

    public void SetCulture(CultureInfo culture)
    {
        _currentCulture = culture;
    }

    public string GetMessage(string errorCode, params object?[] args)
    {
        var cultureName = _currentCulture.TwoLetterISOLanguageName;

        if (!_messages.TryGetValue(cultureName, out var cultureMessages))
        {
            cultureMessages = _messages["en"];
        }

        if (!cultureMessages.TryGetValue(errorCode, out var messageTemplate))
        {
            return $"Unknown error: {errorCode}";
        }

        try
        {
            return string.Format(messageTemplate, args);
        }
        catch
        {
            return messageTemplate;
        }
    }

    public void RegisterMessages(string cultureName, Dictionary<string, string> messages)
    {
        if (!_messages.ContainsKey(cultureName))
        {
            _messages[cultureName] = new Dictionary<string, string>();
        }

        foreach (var kvp in messages)
        {
            _messages[cultureName][kvp.Key] = kvp.Value;
        }
    }
}
