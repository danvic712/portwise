namespace Portwise.Application.Preferences.Dtos;

/// <summary>
/// Returns the persisted application language and theme.
/// </summary>
/// <param name="LanguageCode">Stable application language code.</param>
/// <param name="ThemeCode">Stable application theme code.</param>
/// <param name="Revision">Current optimistic concurrency revision.</param>
/// <param name="UpdatedAtUtc">Time at which the preferences were last updated.</param>
public sealed record PreferencesResponse(
    string LanguageCode,
    string ThemeCode,
    long Revision,
    DateTimeOffset UpdatedAtUtc);
