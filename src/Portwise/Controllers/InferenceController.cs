using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Portwise.Application.Inference.Contracts;
using Portwise.Application.Inference.Dtos;

namespace Portwise.Controllers;

/// <summary>
/// Manages OpenAI-compatible inference providers and capability routes.
/// </summary>
[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/inference")]
public sealed class InferenceController(
    IInferenceConfigurationAppService application) : ControllerBase
{
    /// <summary>Returns all configured inference providers.</summary>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    [HttpGet("providers")]
    [ProducesResponseType<InferenceProvidersResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<InferenceProvidersResponse>> GetProviders(
        CancellationToken cancellationToken)
    {
        var response = await application.GetProvidersAsync(cancellationToken);
        return Ok(response);
    }

    /// <summary>Creates one OpenAI-compatible inference provider.</summary>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    /// <param name="request">Provider name, Base URL, and explicit API key action.</param>
    [HttpPost("providers")]
    [ProducesResponseType<InferenceProviderDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<InferenceProviderDto>> CreateProvider(
        [FromBody] CreateInferenceProviderRequest request,
        CancellationToken cancellationToken)
    {
        var response = await application.CreateProviderAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetProviders),
            new { version = "1" },
            response);
    }

    /// <summary>Updates one inference provider.</summary>
    /// <param name="providerId">Inference provider identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    /// <param name="request">Provider values, API key action, and expected revision.</param>
    [HttpPut("providers/{providerId:guid}")]
    [ProducesResponseType<InferenceProviderDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<InferenceProviderDto>> UpdateProvider(
        Guid providerId,
        [FromBody] UpdateInferenceProviderRequest request,
        CancellationToken cancellationToken)
    {
        var response = await application.UpdateProviderAsync(
            providerId,
            request,
            cancellationToken);
        return Ok(response);
    }

    /// <summary>Deletes an inference provider that is not bound to a route.</summary>
    /// <param name="providerId">Inference provider identifier.</param>
    /// <param name="expectedRevision">Revision last read by the caller.</param>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    [HttpDelete("providers/{providerId:guid}")]
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

    /// <summary>Verifies every complete route bound to one saved provider.</summary>
    /// <param name="providerId">Inference provider identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    /// <param name="request">Expected provider revision.</param>
    [HttpPost("providers/{providerId:guid}/verify")]
    [ProducesResponseType<VerifyInferenceProviderResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<VerifyInferenceProviderResponse>> VerifyProvider(
        Guid providerId,
        [FromBody] VerifyInferenceProviderRequest request,
        CancellationToken cancellationToken)
    {
        var response = await application.VerifyProviderAsync(
            providerId,
            request,
            cancellationToken);
        return Ok(response);
    }

    /// <summary>Returns the Chat and Embedding inference routes.</summary>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    [HttpGet("routes")]
    [ProducesResponseType<InferenceRoutesResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<InferenceRoutesResponse>> GetRoutes(
        CancellationToken cancellationToken)
    {
        var response = await application.GetRoutesAsync(cancellationToken);
        return Ok(response);
    }

    /// <summary>Atomically updates the Chat and Embedding inference routes.</summary>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    /// <param name="request">Both route bindings and their expected revisions.</param>
    [HttpPut("routes")]
    [ProducesResponseType<InferenceRoutesResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<InferenceRoutesResponse>> UpdateRoutes(
        [FromBody] UpdateInferenceRoutesRequest request,
        CancellationToken cancellationToken)
    {
        var response = await application.UpdateRoutesAsync(request, cancellationToken);
        return Ok(response);
    }
}
