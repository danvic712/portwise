using Portwise.Application.Configuration.Dtos;

namespace Portwise.Application.Inference.Dtos;

/// <summary>
/// Requests creation of one OpenAI-compatible inference provider.
/// </summary>
public sealed record CreateInferenceProviderRequest(
    string Name,
    string BaseUrl,
    SecretUpdateRequest ApiKey);
