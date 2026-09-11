using Portwise.Application.Exceptions;
using Portwise.Application.Preferences;
using Portwise.Application.Preferences.Dtos;
using Portwise.Domain.Contracts;
using Portwise.Domain.Enums;
using Portwise.Domain.Exceptions;
using Portwise.Domain.Models;
using Moq;
using Xunit;

namespace Portwise.Application.Tests;

public sealed class PreferencesAppServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 9, 12, 7, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetAsync_ReturnsPersistedCodesAndRevision()
    {
        var preference = ApplicationPreference.Create(
            ApplicationLanguage.ZhCn,
            ApplicationTheme.System,
            DateTimeOffset.UnixEpoch);
        var (service, _) = CreateService(preference);

        var response = await service.GetAsync(CancellationToken.None);

        Assert.Equal("zh-CN", response.LanguageCode);
        Assert.Equal("system", response.ThemeCode);
        Assert.Equal(1, response.Revision);
    }

    [Fact]
    public async Task GetAsync_RejectsMissingPreferences()
    {
        var (service, _) = CreateService(preference: null);

        var exception = await Assert.ThrowsAsync<ApplicationErrorException>(
            () => service.GetAsync(CancellationToken.None));

        Assert.Equal(ApplicationErrorCodes.PreferencesNotConfigured, exception.ErrorCode);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesTrackedPreferenceAndRevision()
    {
        var preference = ApplicationPreference.Create(
            ApplicationLanguage.ZhCn,
            ApplicationTheme.System,
            DateTimeOffset.UnixEpoch);
        var (service, unitOfWork) = CreateService(preference);

        var response = await service.UpdateAsync(
            new UpdatePreferencesRequest("en-US", "dark", 1),
            CancellationToken.None);

        Assert.Equal("en-US", response.LanguageCode);
        Assert.Equal("dark", response.ThemeCode);
        Assert.Equal(2, response.Revision);
        Assert.Equal(Now, response.UpdatedAtUtc);
        unitOfWork.Verify(x => x.CommitAsync(CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData("fr-FR", "system", 1)]
    [InlineData("zh-CN", "blue", 1)]
    [InlineData("zh-CN", "system", 0)]
    public async Task UpdateAsync_RejectsInvalidRequest(
        string languageCode,
        string themeCode,
        long expectedRevision)
    {
        var (service, unitOfWork) = CreateService(preference: null);

        var exception = await Assert.ThrowsAsync<ApplicationErrorException>(() =>
            service.UpdateAsync(
                new UpdatePreferencesRequest(languageCode, themeCode, expectedRevision),
                CancellationToken.None));

        Assert.Equal(ApplicationErrorCodes.PreferencesValidationFailed, exception.ErrorCode);
        unitOfWork.Verify(x => x.Get<ApplicationPreference>(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_RejectsStaleRevisionWithoutCommit()
    {
        var preference = ApplicationPreference.Create(
            ApplicationLanguage.ZhCn,
            ApplicationTheme.System,
            DateTimeOffset.UnixEpoch);
        preference.Update(ApplicationLanguage.EnUs, ApplicationTheme.Light, Now.AddMinutes(-1));
        var (service, unitOfWork) = CreateService(preference);

        var exception = await Assert.ThrowsAsync<ApplicationErrorException>(() =>
            service.UpdateAsync(
                new UpdatePreferencesRequest("zh-CN", "dark", 1),
                CancellationToken.None));

        Assert.Equal(ApplicationErrorCodes.PreferencesRevisionConflict, exception.ErrorCode);
        unitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_MapsDatabaseConcurrencyConflict()
    {
        var preference = ApplicationPreference.Create(
            ApplicationLanguage.ZhCn,
            ApplicationTheme.System,
            DateTimeOffset.UnixEpoch);
        var (service, unitOfWork) = CreateService(preference);
        unitOfWork
            .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnitOfWorkCommitException(
                new InvalidOperationException(),
                isConcurrencyConflict: true));

        var exception = await Assert.ThrowsAsync<ApplicationErrorException>(() =>
            service.UpdateAsync(
                new UpdatePreferencesRequest("en-US", "light", 1),
                CancellationToken.None));

        Assert.Equal(ApplicationErrorCodes.PreferencesRevisionConflict, exception.ErrorCode);
    }

    private static (PreferencesAppService Service, Mock<IUow> UnitOfWork) CreateService(
        ApplicationPreference? preference)
    {
        var repository = RepositoryMock.Create<ApplicationPreference>(
            preference is null ? [] : [preference]);
        var unitOfWork = new Mock<IUow>();
        unitOfWork
            .Setup(x => x.Get<ApplicationPreference>())
            .Returns(repository.Object);
        unitOfWork
            .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        return (new PreferencesAppService(unitOfWork.Object, new FixedTimeProvider(Now)), unitOfWork);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
