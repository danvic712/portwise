namespace Portwise.Application.Setup.Dtos;

public sealed record SetupResult(
    Guid PortfolioId,
    string PortfolioName,
    bool StockDataSyncScheduled,
    IReadOnlyList<SetupStockResult> Stocks);
