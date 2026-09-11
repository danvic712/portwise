using Portwise.Domain.Codes;
using Portwise.Domain.Enums;
using Portwise.Domain.Models;
using Xunit;

namespace Portwise.Domain.Tests;

public sealed class ApplicationPreferenceTests
{
    [Fact]
    public void Create_UsesStableUuidV7AndInitialRevision()
    {
        var timestamp = new DateTimeOffset(2026, 9, 12, 8, 0, 0, TimeSpan.FromHours(8));

        var preference = ApplicationPreference.Create(
            ApplicationLanguage.ZhCn,
            ApplicationTheme.System,
            timestamp);

        Assert.Equal(KnownConfigurationIds.ApplicationPreferences, preference.Id);
        Assert.Equal('7', preference.Id.ToString("D")[14]);
        Assert.Equal(1, preference.Revision);
        Assert.Equal(timestamp.ToUniversalTime(), preference.CreatedAtUtc);
        Assert.Equal(timestamp.ToUniversalTime(), preference.UpdatedAtUtc);
    }

    [Fact]
    public void Update_ChangesValuesAndAdvancesRevision()
    {
        var preference = ApplicationPreference.Create(
            ApplicationLanguage.ZhCn,
            ApplicationTheme.System,
            DateTimeOffset.UnixEpoch);
        var updatedAt = DateTimeOffset.UnixEpoch.AddHours(2);

        preference.Update(ApplicationLanguage.EnUs, ApplicationTheme.Dark, updatedAt);

        Assert.Equal(ApplicationLanguage.EnUs, preference.Language);
        Assert.Equal(ApplicationTheme.Dark, preference.Theme);
        Assert.Equal(2, preference.Revision);
        Assert.Equal(updatedAt, preference.UpdatedAtUtc);
    }
}
