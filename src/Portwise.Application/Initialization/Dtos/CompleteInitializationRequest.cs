namespace Portwise.Application.Initialization.Dtos;

/// <summary>
/// Supplies preferences, the unique portfolio, its optional initial stocks, and external capabilities.
/// </summary>
/// <param name="LanguageCode">The language selected for the workspace.</param>
/// <param name="ThemeCode">The display theme selected for the workspace.</param>
/// <param name="PortfolioName">The name of the initial portfolio.</param>
/// <param name="StockDataProviders">Optional stock data provider configurations.</param>
/// <param name="StockDataRoutes">Optional stock data capability routes.</param>
/// <param name="InferenceProviders">Optional keys for existing inference providers.</param>
/// <param name="InferenceRoutes">Optional Chat and Embedding routes, which may use different providers.</param>
/// <param name="InitialStocks">Optional initial stocks saved with the portfolio.</param>
public sealed record CompleteInitializationRequest(
    string LanguageCode,
    string ThemeCode,
    string PortfolioName,
    IReadOnlyList<CompleteStockDataProviderRequest>? StockDataProviders,
    IReadOnlyList<CompleteStockDataRouteRequest>? StockDataRoutes,
    IReadOnlyList<CompleteInferenceProviderRequest>? InferenceProviders,
    IReadOnlyList<CompleteInferenceRouteRequest>? InferenceRoutes,
    IReadOnlyList<InitialStockRequest>? InitialStocks = null);
