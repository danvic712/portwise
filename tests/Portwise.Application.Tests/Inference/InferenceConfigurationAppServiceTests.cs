using Moq;
using Portwise.Application.Configuration;
using Portwise.Application.Configuration.Contracts;
using Portwise.Application.Configuration.Dtos;
using Portwise.Application.Exceptions;
using Portwise.Application.Inference;
using Portwise.Application.Inference.Contracts;
using Portwise.Application.Inference.Dtos;
using Portwise.Domain.Contracts;
using Portwise.Domain.Enums;
using Portwise.Domain.Models;
using Xunit;

namespace Portwise.Application.Tests;

public sealed class InferenceConfigurationAppServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 9, 12, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task CreateProviderAsync_ProtectsKeyAndUsesVersion7Id()
    {
        var fixture = CreateFixture();

        var response = await fixture.Service.CreateProviderAsync(
            new CreateInferenceProviderRequest(
                " Primary AI ",
                "https://ai.example/v1/",
                new SecretUpdateRequest("replace", "plain-key")),
            CancellationToken.None);

        Assert.Equal("Primary AI", response.Name);
        Assert.Equal("https://ai.example/v1", response.BaseUrl);
        Assert.Equal("openai-compatible", response.ProviderTypeCode);
        Assert.Equal("configured", response.SecretState.StateCode);
        Assert.Equal("configured-unverified", response.RuntimeStatusCode);
        Assert.Equal(7, response.Id.Version);
        fixture.SecretProtector.Verify(protector => protector.Protect(
            "plain-key",
            SecretProtectionPurpose.InferenceProviderApiKey), Times.Once);
    }

    [Fact]
    public async Task CreateProviderAsync_AllowsSameBaseUrlButRejectsCaseInsensitiveName()
    {
        var provider = CreateProvider("Primary", "https://ai.example/v1");
        var fixture = CreateFixture(providers: [provider]);

        var exception = await Assert.ThrowsAsync<ApplicationErrorException>(() =>
            fixture.Service.CreateProviderAsync(
                new CreateInferenceProviderRequest(
                    "primary",
                    "https://ai.example/v1",
                    new SecretUpdateRequest("clear", null)),
                CancellationToken.None));

        Assert.Equal(ApplicationErrorCodes.InferenceProviderNameConflict, exception.ErrorCode);
        fixture.UnitOfWork.Verify(
            unit => unit.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateProviderAsync_NameOnlyPreservesVerification()
    {
        var provider = CreateProvider("Primary", "https://ai.example/v1");
        provider.MarkVerificationSucceeded(Now.AddMinutes(-5));
        var fixture = CreateFixture(providers: [provider]);

        var response = await fixture.Service.UpdateProviderAsync(
            provider.Id,
            new UpdateInferenceProviderRequest(
                "Renamed",
                provider.BaseUrl,
                new SecretUpdateRequest("keep", null),
                provider.Revision),
            CancellationToken.None);

        Assert.Equal("recently-verified", response.RuntimeStatusCode);
        Assert.Equal("succeeded", response.VerificationStateCode);
        Assert.Equal("RENAMED", provider.NormalizedName);
    }

    [Fact]
    public async Task UpdateProviderAsync_BaseUrlChangeResetsVerification()
    {
        var provider = CreateProvider("Primary", "https://ai.example/v1");
        provider.MarkVerificationSucceeded(Now.AddMinutes(-5));
        var fixture = CreateFixture(providers: [provider]);

        var response = await fixture.Service.UpdateProviderAsync(
            provider.Id,
            new UpdateInferenceProviderRequest(
                provider.Name,
                "https://other.example/v1",
                new SecretUpdateRequest("keep", null),
                provider.Revision),
            CancellationToken.None);

        Assert.Equal("configured-unverified", response.RuntimeStatusCode);
        Assert.Equal("unverified", response.VerificationStateCode);
        Assert.Null(response.LastVerifiedAtUtc);
    }

    [Fact]
    public async Task DeleteProviderAsync_RejectsProviderBoundToCapability()
    {
        var provider = CreateProvider("Primary", "https://ai.example/v1");
        var route = CreateRoute(InferenceCapability.Chat, provider.Id, "chat-model");
        var fixture = CreateFixture(providers: [provider], routes: [route]);

        var exception = await Assert.ThrowsAsync<ApplicationErrorException>(() =>
            fixture.Service.DeleteProviderAsync(
                provider.Id,
                provider.Revision,
                CancellationToken.None));

        Assert.Equal(ApplicationErrorCodes.InferenceProviderInUse, exception.ErrorCode);
        fixture.UnitOfWork.Verify(
            unit => unit.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateRoutesAsync_BindsChatAndEmbeddingIndependentlyInOneCommit()
    {
        var chatProvider = CreateProvider("Chat", "https://chat.example/v1");
        var embeddingProvider = CreateProvider("Embedding", "https://embedding.example/v1");
        var routes = new[]
        {
            CreateRoute(InferenceCapability.Chat, null, null),
            CreateRoute(InferenceCapability.Embedding, null, null)
        };
        var fixture = CreateFixture(
            providers: [chatProvider, embeddingProvider],
            routes: routes);

        var response = await fixture.Service.UpdateRoutesAsync(
            new UpdateInferenceRoutesRequest(
            [
                new("chat", chatProvider.Id, "chat-model", 1),
                new("embedding", embeddingProvider.Id, "embedding-model", 1)
            ]),
            CancellationToken.None);

        Assert.Equal(chatProvider.Id, response.Routes.Single(x => x.CapabilityCode == "chat").ProviderId);
        Assert.Equal(
            embeddingProvider.Id,
            response.Routes.Single(x => x.CapabilityCode == "embedding").ProviderId);
        Assert.All(response.Routes, route => Assert.Equal("configured-unverified", route.RuntimeStatusCode));
        fixture.UnitOfWork.Verify(unit => unit.CommitAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task UpdateRoutesAsync_ModelChangeResetsProviderVerification()
    {
        var provider = CreateProvider("Primary", "https://ai.example/v1");
        provider.MarkVerificationSucceeded(Now.AddMinutes(-5));
        var routes = new[]
        {
            CreateRoute(InferenceCapability.Chat, provider.Id, "old-model"),
            CreateRoute(InferenceCapability.Embedding, null, null)
        };
        var fixture = CreateFixture(providers: [provider], routes: routes);

        await fixture.Service.UpdateRoutesAsync(
            new UpdateInferenceRoutesRequest(
            [
                new("chat", provider.Id, "new-model", 1),
                new("embedding", null, null, 1)
            ]),
            CancellationToken.None);

        Assert.Equal(ProviderVerificationState.Unverified, provider.VerificationState);
        Assert.Null(provider.LastVerifiedAtUtc);
        Assert.Equal(3, provider.Revision);
    }

    [Fact]
    public async Task UpdateRoutesAsync_RejectsProviderWithoutModel()
    {
        var provider = CreateProvider("Primary", "https://ai.example/v1");
        var routes = new[]
        {
            CreateRoute(InferenceCapability.Chat, null, null),
            CreateRoute(InferenceCapability.Embedding, null, null)
        };
        var fixture = CreateFixture(providers: [provider], routes: routes);

        var exception = await Assert.ThrowsAsync<ApplicationErrorException>(() =>
            fixture.Service.UpdateRoutesAsync(
                new UpdateInferenceRoutesRequest(
                [
                    new("chat", provider.Id, null, 1),
                    new("embedding", null, null, 1)
                ]),
                CancellationToken.None));

        Assert.Equal(ApplicationErrorCodes.InferenceRouteValidationFailed, exception.ErrorCode);
    }

    [Fact]
    public async Task VerifyProviderAsync_RecordsConnectionFailureWithoutChangingConfiguration()
    {
        var provider = CreateProvider("Primary", "https://ai.example/v1");
        var fixture = CreateFixture(providers: [provider], verificationResult: false);

        var response = await fixture.Service.VerifyProviderAsync(
            provider.Id,
            new VerifyInferenceProviderRequest(provider.Revision),
            CancellationToken.None);

        Assert.Equal("currently-unavailable", response.Provider.RuntimeStatusCode);
        Assert.Equal("inference_provider_connection_failed", response.Provider.LastVerificationErrorCode);
        Assert.Equal("https://ai.example/v1", response.Provider.BaseUrl);
        Assert.Equal("configured", response.Provider.SecretState.StateCode);
    }

    [Fact]
    public async Task VerifyProviderAsync_NoCompleteRouteLeavesProviderUnverified()
    {
        var provider = CreateProvider("Primary", "https://ai.example/v1");
        var fixture = CreateFixture(providers: [provider], verificationResult: null);

        var response = await fixture.Service.VerifyProviderAsync(
            provider.Id,
            new VerifyInferenceProviderRequest(provider.Revision),
            CancellationToken.None);

        Assert.Equal("configured-unverified", response.Provider.RuntimeStatusCode);
        fixture.UnitOfWork.Verify(
            unit => unit.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Fixture CreateFixture(
        IReadOnlyList<InferenceProvider>? providers = null,
        IReadOnlyList<InferenceRoute>? routes = null,
        bool? verificationResult = true)
    {
        var providerRepository = RepositoryMock.Create(providers ?? []);
        var routeRepository = RepositoryMock.Create(routes ?? []);
        var unitOfWork = new Mock<IUow>();
        unitOfWork.Setup(unit => unit.Get<InferenceProvider>())
            .Returns(providerRepository.Object);
        unitOfWork.Setup(unit => unit.Get<InferenceRoute>())
            .Returns(routeRepository.Object);
        unitOfWork.Setup(unit => unit.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var protector = new Mock<ISecretProtector>();
        protector.Setup(item => item.Protect(
                It.IsAny<string>(),
                SecretProtectionPurpose.InferenceProviderApiKey))
            .Returns((string plaintext, SecretProtectionPurpose _) => $"protected:{plaintext}");
        protector.Setup(item => item.TryUnprotect(
                It.IsAny<string>(),
                SecretProtectionPurpose.InferenceProviderApiKey,
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

        var verifier = new Mock<IInferenceProviderConnectionVerifier>();
        verifier.Setup(item => item.VerifyAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(verificationResult);
        var service = new InferenceConfigurationAppService(
            unitOfWork.Object,
            protector.Object,
            verifier.Object,
            new FixedTimeProvider(Now));
        return new Fixture(service, unitOfWork, protector);
    }

    private static InferenceProvider CreateProvider(string name, string baseUrl) =>
        InferenceProvider.Create(
            name,
            baseUrl,
            "protected:key",
            DateTimeOffset.UnixEpoch);

    private static InferenceRoute CreateRoute(
        InferenceCapability capability,
        Guid? providerId,
        string? modelName) => new()
    {
        Id = Guid.CreateVersion7(),
        Capability = capability,
        ProviderId = providerId,
        ModelName = modelName,
        Revision = 1,
        UpdatedAtUtc = DateTimeOffset.UnixEpoch
    };

    private sealed record Fixture(
        InferenceConfigurationAppService Service,
        Mock<IUow> UnitOfWork,
        Mock<ISecretProtector> SecretProtector);

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
