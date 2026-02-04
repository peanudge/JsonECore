namespace JsonECore.Localization;

/// <summary>
/// Interface for providing localized messages.
/// </summary>
public interface ILocalizationProvider
{
    string GetMessage(string errorCode, params object?[] args);
}
