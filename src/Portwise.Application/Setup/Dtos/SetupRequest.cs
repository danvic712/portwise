namespace Portwise.Application.Setup.Dtos;

public sealed record SetupRequest(
    string PortfolioName,
    IReadOnlyList<SetupStockRequest> Stocks);
