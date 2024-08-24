using System.Diagnostics;
using System.Diagnostics.Metrics;
using Orders.Models;

namespace Orders.Diagnostics;

public static class ApplicationDiagnosticsExtensions
{
    public static void EnrichWithOrder(this Activity? activity, Order order)
    {
        // make sure these values are primitive types
        activity?.SetTag("order.id", order.Id);
        activity?.SetTag("order.number", order.Number);
    }

    public static void Increase(this Counter<int> counter, Order order)
    {
        counter.Add(delta: 1,
            tags: [new KeyValuePair<string, object?>("order.number", order.Number)]);
    }
}