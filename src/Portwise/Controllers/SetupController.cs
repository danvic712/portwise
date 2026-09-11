using Portwise.Application.Contracts;
using Portwise.Application.Setup.Contracts;
using Portwise.Application.Setup.Dtos;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Portwise.Controllers;

/// <summary>
/// Provides first-run portfolio setup endpoints.
/// </summary>
[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/setup")]
public sealed class SetupController(ISetupAppService setupAppService) : ControllerBase
{
    /// <summary>
    /// Returns whether the initial portfolio setup is complete.
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult<SetupStatusDto>> GetStatus(CancellationToken cancellationToken)
    {
        var status = await setupAppService.GetStatusAsync(cancellationToken);
        return Ok(status);
    }

    /// <summary>
    /// Creates the initial portfolio and configured stock watchlist.
    /// <param name="request">Portfolio name, stocks and optional initial holdings.</param>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<SetupResult>> Initialize(
        [FromBody] SetupRequest request,
        CancellationToken cancellationToken)
    {
        var result = await setupAppService.InitializeAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetStatus),
            new { version = "1" },
            result);
    }
}
