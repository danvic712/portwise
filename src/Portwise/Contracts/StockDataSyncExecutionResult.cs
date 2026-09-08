using Portwise.Application.Dtos;

namespace Portwise.Contracts;

public sealed record StockDataSyncExecutionResult(
    string RunId,
    StockDataSyncRunResult Result);
