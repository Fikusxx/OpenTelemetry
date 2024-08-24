using System.Reflection;
using MassTransit;
using MassTransit.Logging;
using MassTransit.Monitoring;
using Npgsql;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Orders.Diagnostics;

namespace Orders;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        var otlpEndpoint = new Uri(builder.Configuration.GetValue<string>("OTLP_Endpoint")!);

        // logging before OT package 1.9<
        // builder.Logging.Configure(options =>
        //     {
        //         options.ActivityTrackingOptions = ActivityTrackingOptions.SpanId
        //                                           | ActivityTrackingOptions.TraceId
        //                                           | ActivityTrackingOptions.ParentId
        //                                           | ActivityTrackingOptions.Baggage
        //                                           | ActivityTrackingOptions.Tags;
        //     })
        //     .AddOpenTelemetry(options =>
        //         {
        //             options.IncludeScopes = true;
        //             options.IncludeFormattedMessage = true;
        //             options.ParseStateValues = true;
        //
        //             options
        //                 .SetResourceBuilder(ResourceBuilder.CreateDefault()
        //                     .AddService(ApplicationDiagnostics.ServiceName));
        //
        //             options.AddOtlpExporter(loggerOptions =>
        //             {
        //                 loggerOptions.Protocol = OtlpExportProtocol.Grpc;
        //                 loggerOptions.Endpoint = otlpEndpoint;
        //             });
        //         }
        //     );

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource
                    .AddService(serviceName: ApplicationDiagnostics.ServiceName,
                        serviceNamespace: "Orders.Subdomain",
                        serviceVersion: Assembly.GetExecutingAssembly().GetName().Version!.ToString())
                    .AddAttributes(new[]
                    {
                        new KeyValuePair<string, object>("whatever", "value")
                    });
            })
            .WithTracing(tracing =>
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation(
                        options => options.RecordException = true)
                    .AddNpgsql()
                    // MT
                    .AddSource(DiagnosticHeaders.DefaultListenerName)
                    .AddOtlpExporter(options =>
                    {
                        options.Protocol = OtlpExportProtocol.Grpc; // default
                        options.Endpoint = otlpEndpoint;
                    })
            )
            .WithMetrics(metrics =>
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    // local
                    .AddMeter(ApplicationDiagnostics.Meter.Name)
                    // MT
                    .AddMeter(InstrumentationOptions.MeterName)
                    // Metrics provides by ASP.NET
                    .AddMeter("Microsoft.AspNetCore.Hosting")
                    .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                    // .AddPrometheusExporter()
                    .AddOtlpExporter(options =>
                    {
                        options.Protocol = OtlpExportProtocol.Grpc; // default
                        options.Endpoint = otlpEndpoint;
                    })
            )
            // Available starting >=1.9 version of OT package
            .WithLogging(logging =>
                    logging
                        .AddOtlpExporter(options =>
                        {
                            options.Protocol = OtlpExportProtocol.Grpc; // default
                            options.Endpoint = otlpEndpoint;
                        }),
                loggingOptions =>
                {
                    loggingOptions.IncludeScopes = true;
                    loggingOptions.IncludeFormattedMessage = true;
                }
            );

        return builder;
    }

    public static WebApplicationBuilder AddTransport(this WebApplicationBuilder builder)
    {
        var connection = builder.Configuration.GetConnectionString("RabbitMq")!;

        builder.Services.AddMassTransit(cfg =>
        {
            cfg.SetKebabCaseEndpointNameFormatter();

            cfg.UsingRabbitMq((ctx, rabbit) =>
            {
                var uri = new Uri(connection);

                // credentials are default
                rabbit.Host(uri);
                rabbit.ConfigureEndpoints(ctx);
            });
        });

        return builder;
    }
}