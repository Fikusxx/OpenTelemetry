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
        options.AddService(serviceName: Diagnostics.ServiceName, serviceVersion: "1.0");
    })
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();

        metrics.AddMeter(Diagnostics.Meter.Name);
    })
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();
    });

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeScopes = true;
    logging.IncludeFormattedMessage = true;
});

builder.Services.Configure<OpenTelemetryLoggerOptions>(x => x.AddOtlpExporter());
builder.Services.ConfigureOpenTelemetryMeterProvider(x => x.AddOtlpExporter());
builder.Services.ConfigureOpenTelemetryTracerProvider(x => x.AddOtlpExporter());

builder.Services.Configure<HostOptions>(x => x.ShutdownTimeout = TimeSpan.FromSeconds(30));

var app = builder.Build();



app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

//app.UseHttpsRedirection();

app.Run();