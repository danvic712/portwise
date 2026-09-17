using Portwise.Application.Stocks.Dtos;

namespace Portwise.Application.Stocks.Contracts;

public interface IStockDataSyncJobQueue
{
    Task<StockDataSyncJobResponse> EnqueueStockAsync(
        string triggerCode,
        Guid batchId,
        Guid securityId,
        string? deduplicationKey,
        CancellationToken cancellationToken);

    Task<StockDataSyncJobResponse?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<StockDataSyncBatchResponse?> GetBatchAsync(Guid id, CancellationToken cancellationToken);

    Task<StockDataSyncJobLease?> TryClaimAsync(
        Guid ownerId,
        CancellationToken cancellationToken);

    Task<bool> RenewLeaseAsync(
        Guid id,
        Guid ownerId,
        CancellationToken cancellationToken);

    Task CompleteAsync(
        Guid id,
        Guid ownerId,
        StockFactSyncResult result,
        CancellationToken cancellationToken);

    Task FailAsync(
        Guid id,
        Guid ownerId,
        int attemptCount,
        string errorCode,
        CancellationToken cancellationToken);
}
