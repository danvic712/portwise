using Portwise;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using Serilog;

// The first phase of two-stage initialization uses a console-only bootstrap logger
// to capture fatal errors before the host is built (configuration and DI setup).
// AddSerilog replaces it with the complete appsettings-based configuration.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Portwise host");

    var isOpenApiGeneration = Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider";
    if (isOpenApiGeneration)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_HOSTINGSTARTUPASSEMBLIES", null);
        Environment.SetEnvironmentVariable("DOTNET_STARTUP_HOOKS", null);
    }

    var builder = isOpenApiGeneration
        ? WebApplication.CreateEmptyBuilder(new WebApplicationOptions
        {
            ApplicationName = Assembly.GetExecutingAssembly().GetName().Name,
            ContentRootPath = Directory.GetCurrentDirectory(),
            Args = []
        })
        : WebApplication.CreateBuilder(args);
    if (isOpenApiGeneration)
    {
        builder.Services.AddPortwiseOpenApiDescription();
    }
    else
    {
        builder.AddPortwise();
    }

    var app = builder.Build();

    if (isOpenApiGeneration)
    {
        app.MapPortwiseOpenApi();
        app.MapControllers();
        app.Run();
    }
    else
    {
        await app.RunPortwiseAsync();
    }
}
catch (Exception exception) when (exception is not HostAbortedException)
{
    // HostAbortedException is raised by design-time tools such as `dotnet ef`;
    // it is expected control flow and should not be logged as fatal.
    Log.Fatal(exception, "Portwise host terminated unexpectedly");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}

return 0;
