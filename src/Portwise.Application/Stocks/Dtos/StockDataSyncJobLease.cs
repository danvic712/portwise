namespace Portwise.Application.Stocks.Dtos;

public sealed record StockDataSyncJobLease(Guid Id, string TriggerCode, int AttemptCount);
