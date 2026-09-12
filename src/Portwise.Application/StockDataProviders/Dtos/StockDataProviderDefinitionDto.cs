namespace Portwise.Application.StockDataProviders.Dtos;

/// <summary>
/// Describes an available migration-managed stock data provider kind.
/// </summary>
public sealed record StockDataProviderDefinitionDto(
    Guid Id,
    string ProviderKindCode,
    string DisplayName,
    bool IsEnabled);
