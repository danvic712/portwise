using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Portwise.Application.Initialization.Contracts;
using Portwise.Application.Initialization.Dtos;

namespace Portwise.Controllers;

/// <summary>
/// Provides first-run readiness and onboarding endpoints.
/// </summary>
[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/initialization")]
public sealed class InitializationController(
    IInitializationAppService initializationAppService) : ControllerBase
{
    /// <summary>
    /// Returns whether onboarding has been completed and which capabilities are limited.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    [HttpGet]
    [ProducesResponseType<InitializationStatusResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<InitializationStatusResponse>> Get(
        CancellationToken cancellationToken)
        => Ok(await initializationAppService.GetAsync(cancellationToken));

    /// <summary>
    /// Atomically saves the first-run preferences and optional provider bindings.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    /// <param name="request">The preferences, portfolio, and optional provider configuration.</param>
    [HttpPost("complete")]
    [ProducesResponseType<CompleteInitializationResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CompleteInitializationResponse>> Complete(
        [FromBody] CompleteInitializationRequest request,
        CancellationToken cancellationToken)
        => Ok(await initializationAppService.CompleteAsync(request, cancellationToken));
}
