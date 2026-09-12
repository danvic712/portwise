namespace Portwise.Application.Inference.Dtos;

/// <summary>
/// Returns the provider state recorded after a connection verification attempt.
/// </summary>
public sealed record VerifyInferenceProviderResponse(InferenceProviderDto Provider);
