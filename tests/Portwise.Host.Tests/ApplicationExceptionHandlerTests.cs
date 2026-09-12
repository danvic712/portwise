using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Portwise.Application.Contracts;
using Portwise.Application.Diagnostics;
using Portwise.Application.Exceptions;
using Portwise.Application.Localization;
using Portwise.ExceptionHandling;
using Xunit;

namespace Portwise.Host.Tests;

public sealed class ApplicationExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_writes_problem_details_through_framework_service()
    {
        var localizedError = new LocalizedApplicationError(
            ApplicationErrorCodes.InitializationAlreadyCompleted,
            "zh-CN",
            StatusCodes.Status409Conflict,
            "Setup already completed",
            "The portfolio has already been initialized.");
        var localizer = new Mock<IApplicationErrorLocalizer>();
        localizer
            .Setup(x => x.Localize(It.IsAny<ApplicationExceptionBase>(), It.IsAny<string>()))
            .Returns(localizedError);
        var diagnosticContext = new Mock<IDiagnosticContext>();
        DiagnosticScope? capturedScope = null;
        diagnosticContext
            .Setup(x => x.BeginScope(It.IsAny<DiagnosticScope>()))
            .Callback<DiagnosticScope>(scope => capturedScope = scope)
            .Returns(Mock.Of<IDisposable>());

        var services = new ServiceCollection();
        services.AddOptions();
        services.AddProblemDetails();
        using var serviceProvider = services.BuildServiceProvider();
        var responseBody = new MemoryStream();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
            TraceIdentifier = "request-123"
        };
        httpContext.Request.Headers.Accept = "application/problem+json";
        httpContext.Response.Body = responseBody;
        var handler = new ApplicationExceptionHandler(
            localizer.Object,
            diagnosticContext.Object,
            serviceProvider.GetRequiredService<IProblemDetailsService>(),
            NullLogger<ApplicationExceptionHandler>.Instance);

        var handled = await handler.TryHandleAsync(
            httpContext,
            ApplicationErrors.Simple(ApplicationErrorCodes.InitializationAlreadyCompleted),
            CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status409Conflict, httpContext.Response.StatusCode);
        Assert.StartsWith("application/problem+json", httpContext.Response.ContentType, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(capturedScope);
        Assert.Equal("http_error", capturedScope!.Operation);
        Assert.Equal("request-123", capturedScope.CorrelationId);
        Assert.Equal(localizedError.ErrorCode, capturedScope.ErrorCode);
        Assert.Equal("warning", capturedScope.Severity);

        responseBody.Position = 0;
        using var document = await JsonDocument.ParseAsync(responseBody);
        var problemDetails = document.RootElement;
        Assert.Equal(StatusCodes.Status409Conflict, problemDetails.GetProperty("status").GetInt32());
        Assert.Equal(localizedError.Title, problemDetails.GetProperty("title").GetString());
        Assert.Equal(localizedError.Detail, problemDetails.GetProperty("detail").GetString());
        Assert.Equal(localizedError.ErrorCode, problemDetails.GetProperty("error_code").GetString());
        Assert.Equal(localizedError.CultureName, problemDetails.GetProperty("locale").GetString());
        Assert.Equal("request-123", problemDetails.GetProperty("trace_id").GetString());
    }
}
