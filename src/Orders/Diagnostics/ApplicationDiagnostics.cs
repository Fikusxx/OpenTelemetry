using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Orders.Diagnostics;

public static class ApplicationDiagnostics
{
    public const string ServiceName = "Orders.Api";
    public static readonly Meter Meter = new Meter(ServiceName);
    public static readonly Counter<int> OrdersCreatedCounter = Meter.CreateCounter<int>("orders.created");
    public static readonly Histogram<double> Histogram = Meter.CreateHistogram<double>("my-histogram");
    public static readonly ActivitySource ActivitySource = new(ServiceName);
}