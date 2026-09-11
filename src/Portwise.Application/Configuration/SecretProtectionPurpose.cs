namespace Portwise.Application.Configuration;

/// <summary>
/// Identifies an isolated purpose for protecting secret material.
/// </summary>
public enum SecretProtectionPurpose
{
    StockDataProviderCredentials,
    InferenceProviderApiKey
}
