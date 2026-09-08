using Portwise.Application.Contracts;
using Portwise.Application.Recommendations.Contracts;
using Portwise.Application.Recommendations.Dtos;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Portwise.Controllers;

/// <summary>Provides portfolio recommendation and snapshot endpoints.</summary>
[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/recommendations")]
public sealed class RecommendationsController(
    IPortfolioRecommendationAppService portfolioRecommendationAppService,
    IRecommendationSnapshotAppService recommendationSnapshotAppService)
    : ControllerBase
{
    /// <summary>Returns the current portfolio recommendation.</summary>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    [HttpGet]
    public async Task<ActionResult<PortfolioRecommendationResult>> Get(
        CancellationToken cancellationToken)
    {
        var result = await portfolioRecommendationAppService.GetAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>Persists a snapshot of the current recommendation.</summary>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    [HttpPost("snapshots")]
    public async Task<ActionResult<CreateRecommendationSnapshotResult>> CreateSnapshot(
        CancellationToken cancellationToken)
    {
        var result = await recommendationSnapshotAppService.CreateAsync(cancellationToken);
        return Ok(result);
    }
}
