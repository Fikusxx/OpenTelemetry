using System.Diagnostics;
using OpenTelemetry;

namespace OrdersValidator.Diagnostics;

public sealed class OrderBaggageProcessor : BaseProcessor<Activity>
{
    public override void OnStart(Activity data)
    {
        base.OnStart(data);
    }

    public override void OnEnd(Activity data)
    {
        foreach (var (key, value) in Baggage.Current)
        {
            if (key.StartsWith("order."))
                data.SetTag(key, value);
        }

        base.OnEnd(data);
    }
}