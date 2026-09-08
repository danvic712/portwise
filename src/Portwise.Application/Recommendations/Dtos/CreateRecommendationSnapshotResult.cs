namespace Portwise.Application.Recommendations.Dtos;

/// <summary>Result of persisting a portfolio recommendation snapshot.</summary>
/// <param name="ModelRunId">Identifier of the model run.</param>
/// <param name="PortfolioId">Identifier of the portfolio.</param>
/// <param name="SnapshotCount">Number of stock snapshots persisted.</param>
/// <param name="ComputedAt">UTC timestamp when the recommendation was computed.</param>
/// <param name="Stocks">Persisted stock recommendation snapshots.</param>
public sealed record CreateRecommendationSnapshotResult(
    Guid ModelRunId,
    Guid PortfolioId,
    int SnapshotCount,
    DateTimeOffset ComputedAt,
    IReadOnlyList<StockRecommendationResult> Stocks);
