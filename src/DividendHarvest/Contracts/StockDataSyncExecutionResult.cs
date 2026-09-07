using DividendHarvest.Application.Dtos;

namespace DividendHarvest.Contracts;

public sealed record StockDataSyncExecutionResult(
    string RunId,
    StockDataSyncRunResult Result);
