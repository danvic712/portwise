namespace Portwise.Application.Setup.Dtos;

/// <summary>Result of initial portfolio setup.</summary>
/// <param name="PortfolioId">Identifier of the created portfolio.</param>
/// <param name="PortfolioName">Display name of the created portfolio.</param>
/// <param name="StockDataSyncScheduled">Whether initial stock synchronization was scheduled.</param>
/// <param name="Stocks">Stocks created during setup.</param>
public sealed record SetupResult(
    Guid PortfolioId,
    string PortfolioName,
    bool StockDataSyncScheduled,
    IReadOnlyList<SetupStockResult> Stocks);
