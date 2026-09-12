namespace Portwise.Application.Inference.Dtos;

/// <summary>
/// Requests connection verification for the last observed provider revision.
/// </summary>
public sealed record VerifyInferenceProviderRequest(long ExpectedRevision);
