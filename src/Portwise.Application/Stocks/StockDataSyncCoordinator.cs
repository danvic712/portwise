using System.Security.Cryptography;
using System.Text;
using Portwise.Application.Exceptions;
using Portwise.Application.Stocks.Contracts;
using Portwise.Application.Stocks.Dtos;
using Portwise.Domain.Securities;

namespace Portwise.Application.Stocks;

public sealed class StockDataSyncCoordinator(
    IStockWatchlistAppService stockWatchlistAppService,
    IStockDataSyncJobQueue queue,
    TimeProvider timeProvider) : IStockDataSyncCoordinator
{
    public async Task<StockDataSyncBatchResponse> EnqueueWatchlistAsync(
        string triggerCode,
        string? deduplicationPrefix,
        CancellationToken cancellationToken)
    {
        var watchlist = await stockWatchlistAppService.GetAsync(cancellationToken);
        var batchId = CreateBatchId(triggerCode, deduplicationPrefix);
        foreach (var stock in watchlist)
        {
            await queue.EnqueueStockAsync(
                triggerCode,
                batchId,
                stock.SecurityId,
                BuildDeduplicationKey(deduplicationPrefix, stock.SecurityCode, stock.ExchangeCode),
                cancellationToken);
        }

        return await queue.GetBatchAsync(batchId, cancellationToken)
            ?? StockDataSyncBatchResponse.Empty(batchId, triggerCode, timeProvider.GetUtcNow());
    }

    public async Task<StockDataSyncBatchResponse> EnqueueStockAsync(
        string triggerCode,
        string securityCode,
        string exchangeCode,
        CancellationToken cancellationToken)
    {
        AShareReference reference;
        try
        {
            reference = AShareReference.Create(securityCode, exchangeCode);
        }
        catch (ArgumentException)
        {
            throw ApplicationErrors.Validation(
                ApplicationErrorCodes.StockDataSyncValidationFailed,
                "The stock code or exchange is invalid.");
        }
        var stock = (await stockWatchlistAppService.GetAsync(cancellationToken))
            .SingleOrDefault(item => item.SecurityCode == reference.SecurityCode
                && item.ExchangeCode == reference.ExchangeCode);
        if (stock is null)
        {
            throw ApplicationErrors.WithSecurityReference(
                ApplicationErrorCodes.StockNotConfigured,
                reference.SecurityCode,
                reference.ExchangeCode);
        }

        var batchId = CreateBatchId(triggerCode, null);
        await queue.EnqueueStockAsync(
            triggerCode,
            batchId,
            stock.SecurityId,
            null,
            cancellationToken);
        return await queue.GetBatchAsync(batchId, cancellationToken)
            ?? StockDataSyncBatchResponse.Empty(batchId, triggerCode, timeProvider.GetUtcNow());
    }

    public Task<StockDataSyncBatchResponse?> GetBatchAsync(
        Guid batchId,
        CancellationToken cancellationToken)
        => queue.GetBatchAsync(batchId, cancellationToken);

    private static string? BuildDeduplicationKey(
        string? prefix,
        string securityCode,
        string exchangeCode)
        => prefix is null ? null : $"{prefix}:{securityCode}:{exchangeCode}";

    private static Guid CreateBatchId(string triggerCode, string? deduplicationPrefix)
    {
        if (deduplicationPrefix is null)
        {
            return Guid.CreateVersion7();
        }

        var bytes = MD5.HashData(Encoding.UTF8.GetBytes($"{triggerCode}:{deduplicationPrefix}"));
        return new Guid(bytes);
    }
}
