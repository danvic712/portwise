using Portwise;
using Microsoft.Extensions.Hosting;
using Serilog;

// 两阶段初始化的第一阶段：Bootstrap Logger 只写控制台，
// 用于捕获 Host 尚未构建完成前（配置加载、DI 注册）发生的致命错误；
// Host 构建完成后会被 AddSerilog 中读取 appsettings 的完整配置替换。
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Portwise host");

    var builder = WebApplication.CreateBuilder(args);
    builder.AddPortwise();

    var app = builder.Build();

    await app.RunPortwiseAsync();
}
catch (Exception exception) when (exception is not HostAbortedException)
{
    // HostAbortedException 由 `dotnet ef` 等设计时工具触发，属于正常控制流，不应记为致命错误。
    Log.Fatal(exception, "Portwise host terminated unexpectedly");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}

return 0;
