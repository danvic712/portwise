using System.Linq.Expressions;
using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Portwise.Application.Configuration;
using Portwise.Application.Configuration.Contracts;
using Portwise.Domain.Contracts;
using Portwise.Domain.Enums;
using Portwise.Domain.Models;
using Portwise.Infrastructure.Inference;
using Xunit;

namespace Portwise.Infrastructure.Tests;

public sealed class OpenAiCompatibleInferenceProviderConnectionVerifierTests
{
    [Fact]
    public async Task VerifyAsync_SendsMinimalRequestsForEveryBoundCapability()
    {
        var provider = CreateProvider();
        var routes = new[]
        {
            CreateRoute(InferenceCapability.Chat, provider.Id, "chat-model"),
            CreateRoute(InferenceCapability.Embedding, provider.Id, "embedding-model")
        };
        var handler = new RecordingHandler(HttpStatusCode.OK);
        var verifier = CreateVerifier(provider, routes, handler);

        var result = await verifier.VerifyAsync(provider.Id, CancellationToken.None);

        Assert.True(result);
        Assert.Equal(2, handler.Requests.Count);
        Assert.Equal(
            "https://ai.example/v1/chat/completions",
            handler.Requests[0].Uri.AbsoluteUri);
        Assert.Contains("\"model\":\"chat-model\"", handler.Requests[0].Body);
        Assert.Contains("\"max_tokens\":1", handler.Requests[0].Body);
        Assert.Equal(
            "https://ai.example/v1/embeddings",
            handler.Requests[1].Uri.AbsoluteUri);
        Assert.Contains("\"model\":\"embedding-model\"", handler.Requests[1].Body);
        Assert.All(handler.Requests, request => Assert.Equal("Bearer api-key", request.Authorization));
    }

    [Fact]
    public async Task VerifyAsync_ReturnsNullWhenNoCompleteRouteIsBound()
    {
        var provider = CreateProvider();
        var handler = new RecordingHandler(HttpStatusCode.OK);
        var verifier = CreateVerifier(provider, [], handler);

        var result = await verifier.VerifyAsync(provider.Id, CancellationToken.None);

        Assert.Null(result);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task VerifyAsync_ReturnsFalseForNonSuccessResponse()
    {
        var provider = CreateProvider();
        var handler = new RecordingHandler(HttpStatusCode.Unauthorized);
        var verifier = CreateVerifier(
            provider,
            [CreateRoute(InferenceCapability.Chat, provider.Id, "chat-model")],
            handler);

        var result = await verifier.VerifyAsync(provider.Id, CancellationToken.None);

        Assert.False(result);
        Assert.Single(handler.Requests);
    }

    private static OpenAiCompatibleInferenceProviderConnectionVerifier CreateVerifier(
        InferenceProvider provider,
        IReadOnlyList<InferenceRoute> routes,
        HttpMessageHandler handler)
    {
        var providerRepository = new Mock<IRepository<InferenceProvider>>();
        providerRepository.Setup(repository => repository.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<InferenceProvider, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<bool>()))
            .ReturnsAsync(provider);
        var routeRepository = new Mock<IRepository<InferenceRoute>>();
        routeRepository.Setup(repository => repository.ListAsync(
                It.IsAny<Expression<Func<InferenceRoute, bool>>?>(),
                It.IsAny<IReadOnlyList<Expression<Func<InferenceRoute, object>>>?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<bool>()))
            .ReturnsAsync(routes);
        var unitOfWork = new Mock<IUow>();
        unitOfWork.Setup(unit => unit.Get<InferenceProvider>())
            .Returns(providerRepository.Object);
        unitOfWork.Setup(unit => unit.Get<InferenceRoute>())
            .Returns(routeRepository.Object);

        var protector = new Mock<ISecretProtector>();
        string? plaintext = "api-key";
        protector.Setup(item => item.TryUnprotect(
                "protected:key",
                SecretProtectionPurpose.InferenceProviderApiKey,
                out plaintext))
            .Returns(true);
        return new OpenAiCompatibleInferenceProviderConnectionVerifier(
            unitOfWork.Object,
            protector.Object,
            new StubHttpClientFactory(handler),
            TimeProvider.System,
            NullLogger<OpenAiCompatibleInferenceProviderConnectionVerifier>.Instance);
    }

    private static InferenceProvider CreateProvider() => InferenceProvider.Create(
        "Primary",
        "https://ai.example/v1",
        "protected:key",
        DateTimeOffset.UnixEpoch);

    private static InferenceRoute CreateRoute(
        InferenceCapability capability,
        Guid providerId,
        string modelName) => new()
    {
        Capability = capability,
        ProviderId = providerId,
        ModelName = modelName,
        Revision = 1,
        UpdatedAtUtc = DateTimeOffset.UnixEpoch
    };

    private sealed class StubHttpClientFactory(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(handler, disposeHandler: false);
    }

    private sealed class RecordingHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        public List<RecordedRequest> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(new RecordedRequest(
                request.RequestUri!,
                request.Headers.Authorization?.ToString(),
                await request.Content!.ReadAsStringAsync(cancellationToken)));
            return new HttpResponseMessage(statusCode);
        }
    }

    private sealed record RecordedRequest(Uri Uri, string? Authorization, string Body);
}
