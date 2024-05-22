using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetryTest.Meters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(options =>
    {
        options.AddService(Diagnostics.ServiceName);
    })
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();

        metrics.AddMeter(Diagnostics.Meter.Name);

        metrics.AddOtlpExporter(options =>
        {
            // options.Endpoint = new Uri("http://localhost:18889");
            // or set OTEL_EXPORTER_OTLP_ENDPOINT in env variables
        });
    })
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();

        tracing.AddOtlpExporter(options =>
        {
            // options.Endpoint = new Uri("http://localhost:18889");
        });
    });

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.AddOtlpExporter(options =>
    {
        // options.Endpoint = new Uri("http://localhost:18889");
    });
});

var app = builder.Build();



app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

//app.UseHttpsRedirection();

app.Run();