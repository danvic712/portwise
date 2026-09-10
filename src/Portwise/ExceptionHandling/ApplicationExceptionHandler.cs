using System.Globalization;
using Portwise.Application.Contracts;
using Portwise.Application.Diagnostics;
using Portwise.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Portwise.ExceptionHandling;

public sealed class ApplicationExceptionHandler(
    IApplicationErrorLocalizer errorLocalizer,
    IDiagnosticContext diagnosticContext,
    IProblemDetailsService problemDetailsService,
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

        httpContext.Response.StatusCode = localizedError.StatusCode;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = localizedError.StatusCode,
                Title = localizedError.Title,
                Detail = localizedError.Detail,
                Extensions =
                {
                    ["error_code"] = localizedError.ErrorCode,
                    ["locale"] = localizedError.CultureName,
                    ["trace_id"] = httpContext.TraceIdentifier
                }
            }
        });
    }
}
