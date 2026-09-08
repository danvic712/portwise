namespace Portwise.Application.Recommendations.Dtos;

/// <summary>Current market, dividend, position and recommendation analysis for one stock.</summary>
/// <param name="SecurityCode">Security code.</param>
/// <param name="ExchangeCode">Exchange code.</param>
/// <param name="SecurityName">Display name of the security.</param>
/// <param name="ModelStatusCode">Model availability status code.</param>
/// <param name="DividendReliabilityCode">Dividend reliability status code.</param>
/// <param name="ClosePrice">Latest valid close price.</param>
/// <param name="ModelDividendPerShare">Model dividend per share.</param>
/// <param name="DividendModeCode">Dividend mode code, when available.</param>
/// <param name="DividendYield">Latest model dividend yield.</param>
/// <param name="StrongBuyPrice">Strong-buy price threshold.</param>
/// <param name="AccumulatePrice">Accumulation price threshold.</param>
/// <param name="PartialTrimPrice">Partial-trim price threshold.</param>
/// <param name="AggressiveTrimPrice">Aggressive-trim price threshold.</param>
/// <param name="ObservedPriceZoneCode">Latest observed price-zone code.</param>
/// <param name="PriceZoneCode">Confirmed price-zone code.</param>
/// <param name="PriceZoneConfirmed">Whether the price zone is confirmed.</param>
/// <param name="RecommendationCode">Recommendation code.</param>
/// <param name="HeldShares">Current held shares.</param>
/// <param name="CoreShares">Current core shares.</param>
/// <param name="SatelliteShares">Current satellite shares.</param>
/// <param name="DataAsOfDate">Date through which source data is valid.</param>
/// <param name="ModelParameterSetId">Identifier of the applied parameter set.</param>
/// <param name="ComputedAt">UTC timestamp when the analysis was computed.</param>
/// <param name="Explanation">Stable explanation code emitted by the domain.</param>
/// <param name="SecurityId">Persistent identifier of the security.</param>
public sealed record StockAnalysisResult(
    string SecurityCode,
    string ExchangeCode,
    string SecurityName,
    string ModelStatusCode,
    string DividendReliabilityCode,
    decimal? ClosePrice,
    decimal? ModelDividendPerShare,
    string? DividendModeCode,
    decimal? DividendYield,
    decimal? StrongBuyPrice,
    decimal? AccumulatePrice,
    decimal? PartialTrimPrice,
    decimal? AggressiveTrimPrice,
    string? ObservedPriceZoneCode,
    string? PriceZoneCode,
    bool PriceZoneConfirmed,
    string RecommendationCode,
    int HeldShares,
    int CoreShares,
    int SatelliteShares,
    DateOnly? DataAsOfDate,
    Guid? ModelParameterSetId,
    DateTimeOffset ComputedAt,
    string Explanation,
    Guid SecurityId);
