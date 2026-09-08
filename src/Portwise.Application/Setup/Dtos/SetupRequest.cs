namespace Portwise.Application.Setup.Dtos;

/// <summary>Request to initialize the local portfolio workspace.</summary>
/// <param name="PortfolioName">Display name of the portfolio.</param>
/// <param name="Stocks">Stock watchlist to configure.</param>
public sealed record SetupRequest(
    string PortfolioName,
    IReadOnlyList<SetupStockRequest> Stocks);
