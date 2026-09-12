using Portwise.Application.Configuration.Dtos;

namespace Portwise.Application.Inference.Dtos;

/// <summary>
/// Requests an optimistic-concurrency update to one inference provider.
/// </summary>
public sealed record UpdateInferenceProviderRequest(
    string Name,
    string BaseUrl,
    SecretUpdateRequest ApiKey,
    long ExpectedRevision);
