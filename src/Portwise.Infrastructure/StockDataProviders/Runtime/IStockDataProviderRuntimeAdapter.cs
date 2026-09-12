using Portwise.Application.Stocks.Contracts;
using Portwise.Domain.Enums;
using Portwise.Domain.Models;

namespace Portwise.Infrastructure.StockDataProviders.Runtime;

internal interface IStockDataProviderRuntimeAdapter
{
    StockDataProviderKind ProviderKind { get; }

    Task<IStockDataProvider?> CreateAsync(
        StockDataProvider provider,
        CancellationToken cancellationToken);

    Task<bool> VerifyAsync(
        StockDataProvider provider,
        CancellationToken cancellationToken);
}
