using Portwise.Domain.Codes;
using Portwise.Domain.Enums;

namespace Portwise.Domain.Models;

/// <summary>
/// Stores the single user's application-wide language and theme preferences.
/// </summary>
public sealed class ApplicationPreference
{
    public Guid Id { get; set; } = KnownConfigurationIds.ApplicationPreferences;

    public ApplicationLanguage Language { get; set; } = ApplicationLanguage.ZhCn;

    public ApplicationTheme Theme { get; set; } = ApplicationTheme.System;

    public long Revision { get; set; } = 1;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}
