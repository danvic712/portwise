namespace Portwise.Application.Inference.Contracts;

/// <summary>
/// Verifies the configured capability routes for one saved inference provider.
/// </summary>
public interface IInferenceProviderConnectionVerifier
{
    /// <returns>
    /// <see langword="true" /> for success, <see langword="false" /> for a connection failure,
    /// or <see langword="null" /> when no complete route can be verified.
    /// </returns>
    Task<bool?> VerifyAsync(Guid providerId, CancellationToken cancellationToken);
}
