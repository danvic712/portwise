namespace Portwise.Application.StockDataProviders.Dtos;

/// <summary>
/// Returns available provider definitions and configured provider instances.
/// </summary>
public sealed record StockDataProvidersResponse(
    IReadOnlyList<StockDataProviderDefinitionDto> Definitions,
    IReadOnlyList<StockDataProviderDto> Providers);
