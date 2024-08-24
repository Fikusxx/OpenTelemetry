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
using OrdersView.Consumers;
using StackExchange.Redis;

namespace OrdersView;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        const string serviceName = "OrdersView.Api";

        var otlpEndpoint = new Uri(builder.Configuration.GetValue<string>("OTLP_Endpoint")!);

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource
                    .AddService(serviceName,
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
                    .AddHttpClientInstrumentation(
                        options => options.RecordException = true)
                    .AddNpgsql()
                    // MT
                    .AddSource(DiagnosticHeaders.DefaultListenerName)
                    .AddRedisInstrumentation()
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
                    // MT
                    .AddMeter(InstrumentationOptions.MeterName)
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

    public static WebApplicationBuilder AddTransport(this WebApplicationBuilder builder)
    {
        var connection = builder.Configuration.GetConnectionString("RabbitMq")!;

        builder.Services.AddMassTransit(cfg =>
        {
            cfg.SetKebabCaseEndpointNameFormatter();
            cfg.AddConsumer<OrderCreatedEventConsumer>();

            cfg.UsingRabbitMq((ctx, rabbit) =>
            {
                var uri = new Uri(connection);

                // credentials are default
                rabbit.Host(uri);

                rabbit.ReceiveEndpoint("orders-view", opt =>
                {
                    //
                    opt.ConfigureConsumer<OrderCreatedEventConsumer>(ctx);
                });

                rabbit.ConfigureEndpoints(ctx);
            });
        });

        return builder;
    }

    public static WebApplicationBuilder AddRedis(this WebApplicationBuilder builder)
    {
        var connection = builder.Configuration.GetConnectionString("OrdersCache")!;
        IConnectionMultiplexer? connectionMultiplexer = ConnectionMultiplexer.Connect(connection);
        builder.Services.AddSingleton(connectionMultiplexer);
        builder.Services.AddStackExchangeRedisCache(options =>
            options.ConnectionMultiplexerFactory = () => Task.FromResult(connectionMultiplexer));

        return builder;
    }
}