using Portwise.Domain.Codes;
using Portwise.Domain.Enums;

namespace Portwise.Domain.Models;

/// <summary>
/// Stores the single user's application-wide language and theme preferences.
/// </summary>
public sealed class ApplicationPreference
{
    private ApplicationPreference()
    {
    }

    public Guid Id { get; private set; }

    public ApplicationLanguage Language { get; private set; }

    public ApplicationTheme Theme { get; private set; }

    public long Revision { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static ApplicationPreference Create(
        ApplicationLanguage language,
        ApplicationTheme theme,
        DateTimeOffset createdAtUtc)
    {
        EnsureDefined(language, theme);
        var timestamp = createdAtUtc.ToUniversalTime();
        return new ApplicationPreference
        {
            Id = KnownConfigurationIds.ApplicationPreferences,
            Language = language,
            Theme = theme,
            Revision = 1,
            CreatedAtUtc = timestamp,
            UpdatedAtUtc = timestamp
        };
    }

    public void Update(
        ApplicationLanguage language,
        ApplicationTheme theme,
        DateTimeOffset updatedAtUtc)
    {
        EnsureDefined(language, theme);
        Language = language;
        Theme = theme;
        Revision = checked(Revision + 1);
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    private static void EnsureDefined(
        ApplicationLanguage language,
        ApplicationTheme theme)
    {
        if (!Enum.IsDefined(language))
        {
            throw new ArgumentOutOfRangeException(nameof(language), language, null);
        }

        if (!Enum.IsDefined(theme))
        {
            throw new ArgumentOutOfRangeException(nameof(theme), theme, null);
        }
    }
}
