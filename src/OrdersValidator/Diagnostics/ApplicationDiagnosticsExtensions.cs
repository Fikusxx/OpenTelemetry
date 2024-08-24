using System.Diagnostics;
using OpenTelemetry.Trace;

namespace OrdersValidator.Diagnostics;

public static class ApplicationDiagnosticsExtensions
{
    /// <summary>
    /// Exception handling can be more specific with IExceptionHandler pipeline
    /// </summary>
    public static void RecordOrderValidationException(this Activity? activity, Exception ex)
    {
        activity?.SetStatus(Status.Error);
        activity?.RecordException(ex);
        
        // shouldn't really be here, just convenient for now
        ApplicationDiagnostics.OrdersFailedCounter.Add(delta: 1);
    }
}