using Portwise.Application.Contracts;
using Portwise.Application.Recommendations.Contracts;
using Portwise.Application.Recommendations.Dtos;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Portwise.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/recommendations")]
public sealed class RecommendationsController(
    IPortfolioRecommendationAppService portfolioRecommendationAppService,
    IRecommendationSnapshotAppService recommendationSnapshotAppService)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PortfolioRecommendationResult>> Get(
        CancellationToken cancellationToken)
    {
        var result = await portfolioRecommendationAppService.GetAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost("snapshots")]
    public async Task<ActionResult<CreateRecommendationSnapshotResult>> CreateSnapshot(
        CancellationToken cancellationToken)
    {
        var result = await recommendationSnapshotAppService.CreateAsync(cancellationToken);
        return Ok(result);
    }
}
