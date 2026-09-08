using System.Globalization;
using Portwise.Application.Contracts;
using Portwise.Application.Diagnostics;
using Portwise.Application.Exceptions;
using Portwise.Contracts;
using Microsoft.AspNetCore.Diagnostics;

namespace Portwise.ExceptionHandling;

public sealed class ApplicationExceptionHandler(
    IApplicationErrorLocalizer errorLocalizer,
    IDiagnosticContext diagnosticContext,
    IHttpErrorRenderer errorRenderer,
    ILogger<ApplicationExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ApplicationExceptionBase applicationException)
        {
            return false;
        }

        var localizedError = errorLocalizer.Localize(
            applicationException,
            CultureInfo.CurrentUICulture.Name);

        using var diagnosticScope = diagnosticContext.BeginScope(new DiagnosticScope(
            "http_error",
            CorrelationId: httpContext.TraceIdentifier,
            ErrorCode: localizedError.ErrorCode,
            Severity: "warning"));
        var causeType = exception.InnerException?.GetType().Name ?? exception.GetType().Name;
        logger.LogWarning(
            "Application request failed with status code {StatusCode}, error code {ErrorCode}, locale {Locale}, and cause type {CauseType}.",
            localizedError.StatusCode,
            localizedError.ErrorCode,
            localizedError.CultureName,
            causeType);

        return await errorRenderer.RenderAsync(
            httpContext,
            localizedError,
            cancellationToken);
    }
}
