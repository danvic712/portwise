namespace Portwise.Application.StockDataProviders;

/// <summary>
/// Defines stable runtime and verification codes for stock data providers.
/// </summary>
public static class StockDataProviderConfigurationCodes
{
    public const string RuntimeUnconfigured = "unconfigured";
    public const string RuntimeConfiguredUnverified = "configured-unverified";
    public const string RuntimeRecentlyVerified = "recently-verified";
    public const string RuntimeCurrentlyUnavailable = "currently-unavailable";

    public const string ConnectionFailed = "stock_data_provider_connection_failed";
}
