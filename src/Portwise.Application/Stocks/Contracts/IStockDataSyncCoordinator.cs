using Portwise.Application.Stocks.Dtos;

namespace Portwise.Application.Stocks.Contracts;

/// <summary>
/// Creates and reads durable per-stock synchronization batches. Callers do not
/// need to know how individual jobs are persisted or claimed.
/// </summary>
public interface IStockDataSyncCoordinator
{
    Task<StockDataSyncBatchResponse> EnqueueWatchlistAsync(
        string triggerCode,
        string? deduplicationPrefix,
        CancellationToken cancellationToken);

    Task<StockDataSyncBatchResponse> EnqueueStockAsync(
        string triggerCode,
        string securityCode,
        string exchangeCode,
        CancellationToken cancellationToken);

    Task<StockDataSyncBatchResponse?> GetBatchAsync(
        Guid batchId,
        CancellationToken cancellationToken);
}
