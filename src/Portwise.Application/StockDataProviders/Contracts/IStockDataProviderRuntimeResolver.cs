using Portwise.Application.Stocks.Contracts;
using Portwise.Domain.Enums;

namespace Portwise.Application.StockDataProviders.Contracts;

/// <summary>
/// Resolves the saved provider adapter for one stock data capability.
/// </summary>
public interface IStockDataProviderRuntimeResolver
{
    Task<IStockDataProvider?> ResolveAsync(
        StockDataCapability capability,
        CancellationToken cancellationToken);
}
