using Portwise.Application.Contracts;
using Portwise.Application.Portfolio.Contracts;
using Portwise.Application.Portfolio.Dtos;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Portwise.Controllers;

/// <summary>Provides portfolio trade recording endpoints.</summary>
[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/portfolio")]
public sealed class PortfolioController(IPortfolioTradeAppService portfolioTradeAppService)
    : ControllerBase
{
    /// <summary>Records a simulated portfolio trade.</summary>
    /// <param name="request">Trade direction, quantity, price and date.</param>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    [HttpPost("trades")]
    public async Task<ActionResult<PortfolioTradeResult>> RecordTrade(
        [FromBody] RecordPortfolioTradeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await portfolioTradeAppService.RecordAsync(request, cancellationToken);
        return Ok(result);
    }
}
