using Portwise.Domain.Enums;
using Portwise.Domain.Models;
using Xunit;

namespace Portwise.Domain.Tests;

public sealed class StockDataProviderTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 9, 12, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_UsesVersion7IdAndNormalizesState()
    {
        var provider = StockDataProvider.Create(
            Guid.CreateVersion7(),
            " FTShare ",
            "protected",
            Now);

        Assert.Equal(7, provider.Id.Version);
        Assert.Equal("FTShare", provider.Name);
        Assert.Equal(ProviderVerificationState.Unverified, provider.VerificationState);
        Assert.Equal(1, provider.Revision);
    }

    [Fact]
    public void CredentialsChange_ClearsPreviousVerification()
    {
        var provider = StockDataProvider.Create(
            Guid.CreateVersion7(),
            "FTShare",
            "protected:old",
            Now.AddMinutes(-10));
        provider.MarkVerificationFailed("connection_failed", Now.AddMinutes(-5));

        provider.Update("FTShare", true, "protected:new", Now);

        Assert.Equal(ProviderVerificationState.Unverified, provider.VerificationState);
        Assert.Null(provider.LastVerifiedAtUtc);
        Assert.Null(provider.LastVerificationErrorCode);
        Assert.Equal(3, provider.Revision);
    }

    [Fact]
    public void RouteBinding_AdvancesRevision()
    {
        var providerId = Guid.CreateVersion7();
        var route = new StockDataRoute
        {
            Capability = StockDataCapability.Market,
            Revision = 1,
            UpdatedAtUtc = DateTimeOffset.UnixEpoch
        };

        route.Bind(providerId, Now);

        Assert.Equal(providerId, route.ProviderId);
        Assert.Equal(2, route.Revision);
        Assert.Equal(Now, route.UpdatedAtUtc);
    }
}
