using System.Diagnostics;

namespace Portwise.Diagnostics;

/// <summary>
/// The host's shared <see cref="ActivitySource"/> that creates activities using W3C Trace Context.
/// A basic <see cref="ActivityListener"/> allows activities to be created and recorded without
/// a full OpenTelemetry SDK/exporter; they are currently used for log correlation only.
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
