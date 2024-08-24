using System.Reflection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OrdersValidator.Diagnostics;

namespace OrdersValidator;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        var otlpEndpoint = new Uri(builder.Configuration.GetValue<string>("OTLP_Endpoint")!);

        builder.Services
            .ConfigureOpenTelemetryTracerProvider((_, tracerBuilder) =>
                tracerBuilder.AddProcessor(new OrderBaggageProcessor()));

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource.AddService(ApplicationDiagnostics.ServiceName,
                        "Orders.Subdomain",
                        Assembly.GetExecutingAssembly().GetName().Version!.ToString())
                    .AddAttributes(new[]
                    {
                        new KeyValuePair<string, object>("whatever", "value")
                    });
            })
            .WithTracing(tracing =>
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddGrpcCoreInstrumentation()
                    .AddHttpClientInstrumentation(
                        options => options.RecordException = true)
                    .AddOtlpExporter(options =>
                    {
                        options.Protocol = OtlpExportProtocol.Grpc;
                        options.Endpoint = otlpEndpoint;
                    })
            )
            .WithMetrics(metrics =>
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddMeter(ApplicationDiagnostics.Meter.Name)
                    .AddMeter("Microsoft.AspNetCore.Hosting")
                    .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                    .AddOtlpExporter(options =>
                        options.Endpoint = otlpEndpoint)
            )
            .WithLogging(
                logging =>
                    logging
                        .AddOtlpExporter(options =>
                            options.Endpoint = otlpEndpoint)
            );

        return builder;
    }
}