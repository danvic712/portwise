using System.Diagnostics;

namespace Portwise.Diagnostics;

/// <summary>
/// Host 唯一的 <see cref="ActivitySource"/>，遵循 W3C Trace Context 标准生成 Activity。
/// 静态构造函数注册一个基础 <see cref="ActivityListener"/>，使 Activity 在没有接入完整
/// OpenTelemetry SDK/导出器时也能被创建和记录（当前只用于日志关联，不做跨进程导出）。
/// </summary>
internal static class PortwiseActivitySource
{
    private const string Name = "Portwise";

    public static readonly ActivitySource Instance = new(Name);

    static PortwiseActivitySource()
    {
        ActivitySource.AddActivityListener(new ActivityListener
        {
            ShouldListenTo = source => source.Name == Name,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) =>
                ActivitySamplingResult.AllDataAndRecorded
        });
    }
}
