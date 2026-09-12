namespace Portwise.Application.StockDataProviders.Contracts;

/// <summary>
/// Verifies a saved stock data provider connection without exposing credentials.
/// </summary>
public interface IStockDataProviderConnectionVerifier
{
    Task<bool> VerifyAsync(Guid providerId, CancellationToken cancellationToken);
}
