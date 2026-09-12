namespace Portwise.Application.Initialization.Dtos;

/// <summary>
/// Returns the persisted application language and theme codes.
/// </summary>
public sealed record ApplicationPreferenceDto(
    string LanguageCode,
    string ThemeCode,
    long Revision,
    DateTimeOffset UpdatedAtUtc);
