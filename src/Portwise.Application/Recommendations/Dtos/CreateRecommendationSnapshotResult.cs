namespace Portwise.Application.Recommendations.Dtos;

public sealed record CreateRecommendationSnapshotResult(
    Guid ModelRunId,
    Guid PortfolioId,
    int SnapshotCount,
    DateTimeOffset ComputedAt,
    IReadOnlyList<StockRecommendationResult> Stocks);
