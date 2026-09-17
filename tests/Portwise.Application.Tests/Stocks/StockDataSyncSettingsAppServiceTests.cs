using Moq;
using Portwise.Application.Exceptions;
using Portwise.Application.Stocks;
using Portwise.Application.Stocks.Dtos;
using Portwise.Domain.Contracts;
using Portwise.Domain.Models;
using Xunit;

namespace Portwise.Application.Tests.Stocks;

public sealed class StockDataSyncSettingsAppServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 9, 16, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetAsync_returns_persisted_schedule_and_next_run()
    {
        var settings = StockDataSyncSettings.Create(
            true,
            "UTC",
            "[\"12:00\",\"18:00\"]",
            DateTimeOffset.UnixEpoch);
        var (service, _) = CreateService(settings);

        var response = await service.GetAsync(CancellationToken.None);

        Assert.True(response.Enabled);
        Assert.Equal("UTC", response.TimeZoneId);
        Assert.Equal(["12:00", "18:00"], response.RunTimes);
        Assert.Equal(1, response.Revision);
        Assert.Equal(new DateTimeOffset(2026, 9, 16, 12, 0, 0, TimeSpan.Zero), response.NextRunAtUtc);
    }

    [Fact]
    public async Task UpdateAsync_sorts_run_times_and_increments_revision()
    {
        var settings = StockDataSyncSettings.Create(
            true,
            "UTC",
            "[\"18:00\"]",
            DateTimeOffset.UnixEpoch);
        var (service, unitOfWork) = CreateService(settings);

        var response = await service.UpdateAsync(
            new UpdateStockDataSyncSettingsRequest(
                false,
                "UTC",
                ["18:00", "09:30"],
                1),
            CancellationToken.None);

        Assert.False(response.Enabled);
        Assert.Equal(["09:30", "18:00"], response.RunTimes);
        Assert.Equal(2, response.Revision);
        Assert.Null(response.NextRunAtUtc);
        Assert.Equal("[\"09:30\",\"18:00\"]", settings.RunTimesJson);
        unitOfWork.Verify(x => x.CommitAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_rejects_duplicate_run_times_without_writing()
    {
        var settings = StockDataSyncSettings.Create(
            true,
            "UTC",
            "[\"18:00\"]",
            DateTimeOffset.UnixEpoch);
        var (service, unitOfWork) = CreateService(settings);

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() =>
            service.UpdateAsync(
                new UpdateStockDataSyncSettingsRequest(
                    true,
                    "UTC",
                    ["09:00", "09:00"],
                    1),
                CancellationToken.None));

        Assert.Equal(ApplicationErrorCodes.StockDataSyncValidationFailed, exception.ErrorCode);
        unitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_rejects_stale_revision_without_writing()
    {
        var settings = StockDataSyncSettings.Create(
            true,
            "UTC",
            "[\"18:00\"]",
            DateTimeOffset.UnixEpoch);
        settings.Update(true, "UTC", "[\"19:00\"]", Now.AddMinutes(-1));
        var (service, unitOfWork) = CreateService(settings);

        var exception = await Assert.ThrowsAsync<ApplicationErrorException>(() =>
            service.UpdateAsync(
                new UpdateStockDataSyncSettingsRequest(
                    true,
                    "UTC",
                    ["09:00"],
                    1),
                CancellationToken.None));

        Assert.Equal(ApplicationErrorCodes.StockDataSyncSettingsRevisionConflict, exception.ErrorCode);
        unitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static (StockDataSyncSettingsAppService Service, Mock<IUow> UnitOfWork) CreateService(
        StockDataSyncSettings settings)
    {
        var repository = RepositoryMock.Create([settings]);
        var unitOfWork = new Mock<IUow>();
        unitOfWork.Setup(x => x.Get<StockDataSyncSettings>()).Returns(repository.Object);
        unitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return (new StockDataSyncSettingsAppService(unitOfWork.Object, new FixedTimeProvider(Now)), unitOfWork);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
