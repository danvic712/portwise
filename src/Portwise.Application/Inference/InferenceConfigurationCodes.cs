namespace Portwise.Application.Inference;

/// <summary>
/// Defines stable runtime and verification codes for inference providers.
/// </summary>
public static class InferenceConfigurationCodes
{
    public const string RuntimeUnconfigured = "unconfigured";
    public const string RuntimeConfiguredUnverified = "configured-unverified";
    public const string RuntimeRecentlyVerified = "recently-verified";
    public const string RuntimeCurrentlyUnavailable = "currently-unavailable";

    public const string ConnectionFailed = "inference_provider_connection_failed";
}
