using System.Diagnostics.Metrics;

namespace OpenTelemetryTest.Meters;

public static class Diagnostics
{
    public const string ServiceName = "MyServiceName";
    public static readonly Meter Meter = new(ServiceName);
    public static readonly Counter<int> Counter = Meter.CreateCounter<int>("my-awesome-counter");
}