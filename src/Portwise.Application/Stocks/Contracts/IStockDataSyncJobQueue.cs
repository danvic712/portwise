using Portwise.Application.Stocks.Dtos;

namespace Portwise.Application.Stocks.Contracts;

public interface IStockDataSyncJobQueue
{
    Task<StockDataSyncJobResponse> EnqueueAsync(
        string triggerCode,
        string? deduplicationKey,
        CancellationToken cancellationToken);

    Task<StockDataSyncJobResponse?> GetAsync(Guid id, CancellationToken cancellationToken);

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
        StockDataSyncRunResult result,
        CancellationToken cancellationToken);

    Task FailAsync(
        Guid id,
        Guid ownerId,
        int attemptCount,
        string errorCode,
        CancellationToken cancellationToken);
}
