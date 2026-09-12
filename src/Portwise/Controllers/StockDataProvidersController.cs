using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Portwise.Application.StockDataProviders.Contracts;
using Portwise.Application.StockDataProviders.Dtos;

namespace Portwise.Controllers;

/// <summary>
/// Manages stock data provider connections and capability routes.
/// </summary>
[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/stock-data-providers")]
public sealed class StockDataProvidersController(
    IStockDataProviderConfigurationAppService application) : ControllerBase
{
    /// <summary>Returns available and configured stock data providers.</summary>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    [HttpGet]
    [ProducesResponseType<StockDataProvidersResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<StockDataProvidersResponse>> GetProviders(
        CancellationToken cancellationToken)
    {
        var response = await application.GetProvidersAsync(cancellationToken);
        return Ok(response);
    }

    /// <summary>Creates one stock data provider instance.</summary>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    /// <param name="request">Provider definition, name, and explicit credential action.</param>
    [HttpPost]
    [ProducesResponseType<StockDataProviderDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<StockDataProviderDto>> CreateProvider(
        [FromBody] CreateStockDataProviderRequest request,
        CancellationToken cancellationToken)
    {
        var response = await application.CreateProviderAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetProviders),
            new { version = "1" },
            response);
    }

    /// <summary>Updates one stock data provider instance.</summary>
    /// <param name="providerId">Stock data provider identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    /// <param name="request">Provider values, credential action, and expected revision.</param>
    [HttpPut("{providerId:guid}")]
    [ProducesResponseType<StockDataProviderDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<StockDataProviderDto>> UpdateProvider(
        Guid providerId,
        [FromBody] UpdateStockDataProviderRequest request,
        CancellationToken cancellationToken)
    {
        var response = await application.UpdateProviderAsync(
            providerId,
            request,
            cancellationToken);
        return Ok(response);
    }

    /// <summary>Deletes an unbound stock data provider instance.</summary>
    /// <param name="providerId">Stock data provider identifier.</param>
    /// <param name="expectedRevision">Revision last read by the caller.</param>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    [HttpDelete("{providerId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteProvider(
        Guid providerId,
        [FromQuery] long expectedRevision,
        CancellationToken cancellationToken)
    {
        await application.DeleteProviderAsync(
            providerId,
            expectedRevision,
            cancellationToken);
        return NoContent();
    }

    /// <summary>Verifies the saved connection without changing its configuration.</summary>
    /// <param name="providerId">Stock data provider identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    /// <param name="request">Expected provider revision.</param>
    [HttpPost("{providerId:guid}/verify")]
    [ProducesResponseType<VerifyStockDataProviderResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<VerifyStockDataProviderResponse>> VerifyProvider(
        Guid providerId,
        [FromBody] VerifyStockDataProviderRequest request,
        CancellationToken cancellationToken)
    {
        var response = await application.VerifyProviderAsync(
            providerId,
            request,
            cancellationToken);
        return Ok(response);
    }

    /// <summary>Returns all fixed stock data capability routes.</summary>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    [HttpGet("routes")]
    [ProducesResponseType<StockDataRoutesResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<StockDataRoutesResponse>> GetRoutes(
        CancellationToken cancellationToken)
    {
        var response = await application.GetRoutesAsync(cancellationToken);
        return Ok(response);
    }

    /// <summary>Atomically updates all fixed stock data capability routes.</summary>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    /// <param name="request">All route bindings and their expected revisions.</param>
    [HttpPut("routes")]
    [ProducesResponseType<StockDataRoutesResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<StockDataRoutesResponse>> UpdateRoutes(
        [FromBody] UpdateStockDataRoutesRequest request,
        CancellationToken cancellationToken)
    {
        var response = await application.UpdateRoutesAsync(request, cancellationToken);
        return Ok(response);
    }
}
