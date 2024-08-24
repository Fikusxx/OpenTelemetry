using System.Diagnostics.Metrics;

namespace OrdersValidator.Diagnostics;

public static class ApplicationDiagnostics
{
    public const string ServiceName = "OrdersValidator.Api";
    public static readonly Meter Meter = new Meter(ServiceName);
    public static readonly Counter<int> OrdersFailedCounter = Meter.CreateCounter<int>("orders.failed");
}