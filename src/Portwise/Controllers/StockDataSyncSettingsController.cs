using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Portwise.Application.Stocks.Contracts;
using Portwise.Application.Stocks.Dtos;

namespace Portwise.Controllers;

/// <summary>Manages the persisted daily stock synchronization schedule.</summary>
[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/stock-data-sync")]
public sealed class StockDataSyncSettingsController(
    IStockDataSyncSettingsAppService application) : ControllerBase
{
    /// <summary>Returns the current database-backed synchronization schedule.</summary>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    [HttpGet("settings")]
    [ProducesResponseType<StockDataSyncSettingsResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<StockDataSyncSettingsResponse>> GetSettings(
        CancellationToken cancellationToken)
    {
        var response = await application.GetAsync(cancellationToken);
        return Ok(response);
    }

    /// <summary>Updates the synchronization schedule with optimistic concurrency.</summary>
    /// <param name="request">Enablement, time zone, run times and expected revision.</param>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    [HttpPut("settings")]
    [ProducesResponseType<StockDataSyncSettingsResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<StockDataSyncSettingsResponse>> UpdateSettings(
        [FromBody] UpdateStockDataSyncSettingsRequest request,
        CancellationToken cancellationToken)
    {
        var response = await application.UpdateAsync(request, cancellationToken);
        return Ok(response);
    }
}
