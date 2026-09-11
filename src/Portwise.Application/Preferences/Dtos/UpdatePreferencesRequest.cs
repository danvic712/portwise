namespace Portwise.Application.Preferences.Dtos;

/// <summary>
/// Requests an optimistic-concurrency update to application preferences.
/// </summary>
/// <param name="LanguageCode">Stable application language code.</param>
/// <param name="ThemeCode">Stable application theme code.</param>
/// <param name="ExpectedRevision">Revision last read by the caller.</param>
public sealed record UpdatePreferencesRequest(
    string LanguageCode,
    string ThemeCode,
    long ExpectedRevision);
