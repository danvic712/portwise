using Portwise.Application.Stocks.Dtos;
using Portwise.Domain.Securities;

namespace Portwise.Application.Stocks.Contracts;

public interface IStockDataProvider
{
    Task<StockData?> GetAsync(
        AShareReference reference,
        CancellationToken cancellationToken);

    Task<StockMarketData?> GetMarketDataAsync(
        AShareReference reference,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<StockDividendData>?> GetDividendEventsAsync(
        AShareReference reference,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<StockFinancialData>?> GetFinancialSnapshotsAsync(
        AShareReference reference,
        CancellationToken cancellationToken);
}
