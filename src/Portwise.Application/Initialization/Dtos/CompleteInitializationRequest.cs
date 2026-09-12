namespace Portwise.Application.Initialization.Dtos;

/// <summary>
/// Supplies preferences, the unique portfolio, and optional external capabilities.
/// </summary>
public sealed record CompleteInitializationRequest(
    string LanguageCode,
    string ThemeCode,
    string PortfolioName,
    IReadOnlyList<CompleteStockDataProviderRequest>? StockDataProviders,
    IReadOnlyList<CompleteStockDataRouteRequest>? StockDataRoutes,
    IReadOnlyList<CompleteInferenceProviderRequest>? InferenceProviders,
    IReadOnlyList<CompleteInferenceRouteRequest>? InferenceRoutes);
