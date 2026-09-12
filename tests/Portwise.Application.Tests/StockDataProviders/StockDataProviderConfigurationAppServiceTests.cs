using Moq;
using Portwise.Application.Configuration;
using Portwise.Application.Configuration.Contracts;
using Portwise.Application.Configuration.Dtos;
using Portwise.Application.Exceptions;
using Portwise.Application.StockDataProviders;
using Portwise.Application.StockDataProviders.Contracts;
using Portwise.Application.StockDataProviders.Dtos;
using Portwise.Domain.Contracts;
using Portwise.Domain.Enums;
using Portwise.Domain.Models;
using Xunit;

namespace Portwise.Application.Tests;

public sealed class StockDataProviderConfigurationAppServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 9, 12, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task CreateProviderAsync_ProtectsCredentialsAndReturnsOnlyTheirState()
    {
        var definition = CreateDefinition();
        var fixture = CreateFixture(definitions: [definition]);

        var response = await fixture.Service.CreateProviderAsync(
            new CreateStockDataProviderRequest(
                definition.Id,
                " FTShare primary ",
                new SecretUpdateRequest("replace", "plain-key")),
            CancellationToken.None);

        Assert.Equal("FTShare primary", response.Name);
        Assert.Equal("configured", response.SecretState.StateCode);
        Assert.Equal("configured-unverified", response.RuntimeStatusCode);
        Assert.Equal(7, response.Id.Version);
        fixture.SecretProtector.Verify(protector => protector.Protect(
            "plain-key",
            SecretProtectionPurpose.StockDataProviderCredentials), Times.Once);
        fixture.UnitOfWork.Verify(unit => unit.CommitAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task UpdateProviderAsync_KeepPreservesSuccessfulVerification()
    {
        var definition = CreateDefinition();
        var provider = StockDataProvider.Create(
            definition.Id,
            "FTShare",
            "protected:key",
            DateTimeOffset.UnixEpoch);
        provider.MarkVerificationSucceeded(Now.AddMinutes(-5));
        var fixture = CreateFixture(definitions: [definition], providers: [provider]);

        var response = await fixture.Service.UpdateProviderAsync(
            provider.Id,
            new UpdateStockDataProviderRequest(
                "Renamed",
                new SecretUpdateRequest("keep", null),
                provider.Revision),
            CancellationToken.None);

        Assert.Equal("recently-verified", response.RuntimeStatusCode);
        Assert.Equal("succeeded", response.VerificationStateCode);
        Assert.Equal("protected:key", provider.ProtectedCredentials);
    }

    [Fact]
    public async Task UpdateProviderAsync_ReplaceResetsVerification()
    {
        var definition = CreateDefinition();
        var provider = StockDataProvider.Create(
            definition.Id,
            "FTShare",
            "protected:old",
            DateTimeOffset.UnixEpoch);
        provider.MarkVerificationSucceeded(Now.AddMinutes(-5));
        var fixture = CreateFixture(definitions: [definition], providers: [provider]);

        var response = await fixture.Service.UpdateProviderAsync(
            provider.Id,
            new UpdateStockDataProviderRequest(
                "FTShare",
                new SecretUpdateRequest("replace", "new-key"),
                provider.Revision),
            CancellationToken.None);

        Assert.Equal("configured-unverified", response.RuntimeStatusCode);
        Assert.Equal("unverified", response.VerificationStateCode);
        Assert.Null(response.LastVerifiedAtUtc);
        Assert.Equal("protected:new-key", provider.ProtectedCredentials);
    }

    [Fact]
    public async Task GetProvidersAsync_MarksUnprotectableCredentialsAsUnreadable()
    {
        var definition = CreateDefinition();
        var provider = StockDataProvider.Create(
            definition.Id,
            "FTShare",
            "broken",
            DateTimeOffset.UnixEpoch);
        var fixture = CreateFixture(definitions: [definition], providers: [provider]);

        var response = await fixture.Service.GetProvidersAsync(CancellationToken.None);

        var result = Assert.Single(response.Providers);
        Assert.Equal("unreadable", result.SecretState.StateCode);
        Assert.Equal("unconfigured", result.RuntimeStatusCode);
    }

    [Fact]
    public async Task DeleteProviderAsync_RejectsProviderUsedByRoute()
    {
        var definition = CreateDefinition();
        var provider = StockDataProvider.Create(
            definition.Id,
            "FTShare",
            null,
            DateTimeOffset.UnixEpoch);
        var route = CreateRoute(StockDataCapability.Profile, provider.Id);
        var fixture = CreateFixture(
            definitions: [definition],
            providers: [provider],
            routes: [route]);

        var exception = await Assert.ThrowsAsync<ApplicationErrorException>(() =>
            fixture.Service.DeleteProviderAsync(
                provider.Id,
                provider.Revision,
                CancellationToken.None));

        Assert.Equal(ApplicationErrorCodes.StockDataProviderInUse, exception.ErrorCode);
        fixture.UnitOfWork.Verify(
            unit => unit.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateRoutesAsync_UpdatesAllCapabilityRoutesInOneCommit()
    {
        var definition = CreateDefinition();
        var provider = StockDataProvider.Create(
            definition.Id,
            "FTShare",
            "protected:key",
            DateTimeOffset.UnixEpoch);
        var routes = Enum.GetValues<StockDataCapability>()
            .Select(capability => CreateRoute(capability, null))
            .ToArray();
        var fixture = CreateFixture(
            definitions: [definition],
            providers: [provider],
            routes: routes);

        var response = await fixture.Service.UpdateRoutesAsync(
            new UpdateStockDataRoutesRequest(routes
                .Select(route => new UpdateStockDataRouteRequest(
                    ToCapabilityCode(route.Capability),
                    provider.Id,
                    route.Revision))
                .ToArray()),
            CancellationToken.None);

        Assert.All(response.Routes, route => Assert.Equal(provider.Id, route.ProviderId));
        Assert.All(response.Routes, route => Assert.Equal(2, route.Revision));
        fixture.UnitOfWork.Verify(unit => unit.CommitAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task UpdateRoutesAsync_UsesRouteSpecificRevisionConflict()
    {
        var routes = Enum.GetValues<StockDataCapability>()
            .Select(capability => CreateRoute(capability, null))
            .ToArray();
        var fixture = CreateFixture(routes: routes);

        var exception = await Assert.ThrowsAsync<ApplicationErrorException>(() =>
            fixture.Service.UpdateRoutesAsync(
                new UpdateStockDataRoutesRequest(routes
                    .Select(route => new UpdateStockDataRouteRequest(
                        ToCapabilityCode(route.Capability),
                        null,
                        route.Capability == StockDataCapability.Profile ? 2 : 1))
                    .ToArray()),
                CancellationToken.None));

        Assert.Equal(ApplicationErrorCodes.StockDataRouteRevisionConflict, exception.ErrorCode);
        fixture.UnitOfWork.Verify(
            unit => unit.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Fixture CreateFixture(
        IReadOnlyList<StockDataProviderDefinition>? definitions = null,
        IReadOnlyList<StockDataProvider>? providers = null,
        IReadOnlyList<StockDataRoute>? routes = null)
    {
        var definitionRepository = RepositoryMock.Create(definitions ?? []);
        var providerRepository = RepositoryMock.Create(providers ?? []);
        var routeRepository = RepositoryMock.Create(routes ?? []);
        var unitOfWork = new Mock<IUow>();
        unitOfWork.Setup(unit => unit.Get<StockDataProviderDefinition>())
            .Returns(definitionRepository.Object);
        unitOfWork.Setup(unit => unit.Get<StockDataProvider>())
            .Returns(providerRepository.Object);
        unitOfWork.Setup(unit => unit.Get<StockDataRoute>())
            .Returns(routeRepository.Object);
        unitOfWork.Setup(unit => unit.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var protector = new Mock<ISecretProtector>();
        protector.Setup(item => item.Protect(
                It.IsAny<string>(),
                SecretProtectionPurpose.StockDataProviderCredentials))
            .Returns((string plaintext, SecretProtectionPurpose _) => $"protected:{plaintext}");
        protector.Setup(item => item.TryUnprotect(
                It.IsAny<string>(),
                SecretProtectionPurpose.StockDataProviderCredentials,
                out It.Ref<string?>.IsAny))
            .Returns((string protectedValue, SecretProtectionPurpose _, out string? plaintext) =>
            {
                if (!protectedValue.StartsWith("protected:", StringComparison.Ordinal))
                {
                    plaintext = null;
                    return false;
                }

                plaintext = protectedValue["protected:".Length..];
                return true;
            });

        var verifier = new Mock<IStockDataProviderConnectionVerifier>();
        verifier.Setup(item => item.VerifyAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var service = new StockDataProviderConfigurationAppService(
            unitOfWork.Object,
            protector.Object,
            verifier.Object,
            new FixedTimeProvider(Now));
        return new Fixture(service, unitOfWork, protector);
    }

    private static StockDataProviderDefinition CreateDefinition() => new()
    {
        Id = Guid.CreateVersion7(),
        ProviderKind = StockDataProviderKind.FtShare,
        DisplayName = "FTShare",
        IsEnabled = true
    };

    private static StockDataRoute CreateRoute(
        StockDataCapability capability,
        Guid? providerId) => new()
    {
        Id = Guid.CreateVersion7(),
        Capability = capability,
        ProviderId = providerId,
        Revision = 1,
        UpdatedAtUtc = DateTimeOffset.UnixEpoch
    };

    private static string ToCapabilityCode(StockDataCapability capability) => capability switch
    {
        StockDataCapability.Profile => "profile",
        StockDataCapability.Market => "market",
        StockDataCapability.Dividend => "dividend",
        StockDataCapability.Financial => "financial",
        _ => throw new ArgumentOutOfRangeException(nameof(capability), capability, null)
    };

    private sealed record Fixture(
        StockDataProviderConfigurationAppService Service,
        Mock<IUow> UnitOfWork,
        Mock<ISecretProtector> SecretProtector);

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
