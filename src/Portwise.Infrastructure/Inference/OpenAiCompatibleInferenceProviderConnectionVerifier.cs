using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Portwise.Application.Configuration;
using Portwise.Application.Configuration.Contracts;
using Portwise.Application.Inference.Contracts;
using Portwise.Domain.Contracts;
using Portwise.Domain.Enums;
using Portwise.Domain.Models;

namespace Portwise.Infrastructure.Inference;

internal sealed class OpenAiCompatibleInferenceProviderConnectionVerifier(
    IUow uow,
    ISecretProtector secretProtector,
    IHttpClientFactory httpClientFactory,
    TimeProvider timeProvider,
    ILogger<OpenAiCompatibleInferenceProviderConnectionVerifier> logger)
    : IInferenceProviderConnectionVerifier
{
    internal const string HttpClientName = "OpenAiCompatibleVerification";

    private static readonly TimeSpan VerificationTimeout = TimeSpan.FromSeconds(30);

    public async Task<bool?> VerifyAsync(
        Guid providerId,
        CancellationToken cancellationToken)
    {
        var provider = await uow.Get<InferenceProvider>()
            .SingleOrDefaultAsync(item => item.Id == providerId, cancellationToken);
        if (provider is not { ProviderType: InferenceProviderType.OpenAiCompatible }
            || provider.ProtectedApiKey is not { Length: > 0 } protectedApiKey
            || !TryCreateBaseUri(provider.BaseUrl, out var baseUri)
            || !secretProtector.TryUnprotect(
                protectedApiKey,
                SecretProtectionPurpose.InferenceProviderApiKey,
                out var apiKey)
            || string.IsNullOrWhiteSpace(apiKey))
        {
            return null;
        }

        var routes = await uow.Get<InferenceRoute>().ListAsync(
            route => route.ProviderId == providerId,
            cancellationToken: cancellationToken);
        if (routes.Count == 0
            || routes.Any(route => string.IsNullOrWhiteSpace(route.ModelName)))
        {
            return null;
        }

        using var timeoutCancellation = new CancellationTokenSource(
            VerificationTimeout,
            timeProvider);
        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            timeoutCancellation.Token);
        try
        {
            using var client = httpClientFactory.CreateClient(HttpClientName);
            foreach (var route in routes.OrderBy(route => route.Capability))
            {
                using var request = CreateRequest(
                    baseUri,
                    apiKey,
                    route.Capability,
                    route.ModelName!);
                using var response = await client.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    linkedCancellation.Token);
                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }
            }

            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                "Inference provider verification failed for provider {ProviderId} with cause type {CauseType}.",
                providerId,
                exception.GetType().Name);
            return false;
        }
    }

    private static HttpRequestMessage CreateRequest(
        Uri baseUri,
        string apiKey,
        InferenceCapability capability,
        string modelName)
    {
        var endpoint = capability switch
        {
            InferenceCapability.Chat => new Uri(baseUri, "chat/completions"),
            InferenceCapability.Embedding => new Uri(baseUri, "embeddings"),
            _ => throw new ArgumentOutOfRangeException(nameof(capability), capability, null)
        };
        var content = capability switch
        {
            InferenceCapability.Chat => JsonContent.Create(new
            {
                model = modelName,
                messages = new[] { new { role = "user", content = "ping" } },
                max_tokens = 1
            }),
            InferenceCapability.Embedding => JsonContent.Create(new
            {
                model = modelName,
                input = "ping"
            }),
            _ => throw new ArgumentOutOfRangeException(nameof(capability), capability, null)
        };
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = content
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        return request;
    }

    private static bool TryCreateBaseUri(string baseUrl, out Uri baseUri) =>
        Uri.TryCreate($"{baseUrl.TrimEnd('/')}/", UriKind.Absolute, out baseUri!)
        && baseUri.Scheme is "http" or "https";
}
