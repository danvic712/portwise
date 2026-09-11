using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Portwise.Application.Preferences.Contracts;
using Portwise.Application.Preferences.Dtos;

namespace Portwise.Controllers;

/// <summary>
/// Provides application language and theme preference endpoints.
/// </summary>
[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/preferences")]
public sealed class PreferencesController(IPreferencesAppService preferencesAppService)
    : ControllerBase
{
    /// <summary>
    /// Returns the persisted application preferences.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    [HttpGet]
    public async Task<ActionResult<PreferencesResponse>> Get(
        CancellationToken cancellationToken)
    {
        var response = await preferencesAppService.GetAsync(cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Updates the application language and theme using optimistic concurrency.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    /// <param name="request">Preference values and the expected revision.</param>
    [HttpPut]
    public async Task<ActionResult<PreferencesResponse>> Update(
        [FromBody] UpdatePreferencesRequest request,
        CancellationToken cancellationToken)
    {
        var response = await preferencesAppService.UpdateAsync(request, cancellationToken);
        return Ok(response);
    }
}
