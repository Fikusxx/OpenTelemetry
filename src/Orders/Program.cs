using System.Diagnostics;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry;
using Orders;
using Orders.Contracts;
using Orders.Database;
using Orders.Diagnostics;
using Orders.Models;
using OrderValidators;
using ValidationResult = OrderValidators.ValidationResult;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddDatabase();
builder.AddTransport();
builder.AddObservability();

builder.Services.AddGrpcClient<OrderValidator.OrderValidatorClient>(options =>
{
    options.Address = new Uri(builder.Configuration["OrderValidator:Url"]!);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// expose api (/metrics) for manual scraping metrics
// app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.MapGet("/", () => Results.Ok("Orders Service is working..."));

app.MapPost("/orders", async (
    [FromBody] Order orderRequest,
    [FromServices] OrderValidator.OrderValidatorClient client,
    [FromServices] OrdersDbContext dbContext,
    [FromServices] IPublishEndpoint endpoint,
    [FromServices] ILogger<Program> logger) =>
{
    logger.LogInformation("Validating {OrderNumber}...", orderRequest.Number);
    
    Baggage.SetBaggage("order.id", orderRequest.Id.ToString());
    
    var validationResult = await client.ValidateAsync(new OrderValidationRequest { Number = orderRequest.Number });

    if (validationResult.ValidationResult == ValidationResult.Failed)
    {
        logger.LogInformation("Order {OrderNumber} validation failed...", orderRequest.Number);
        return Results.BadRequest("Order creation failed..");
    }

    Activity.Current.EnrichWithOrder(orderRequest);

    await dbContext.Orders.AddAsync(orderRequest);
    await dbContext.SaveChangesAsync();
    
    logger.LogInformation("Order {OrderNumber} saved...", orderRequest.Number);

    ApplicationDiagnostics.OrdersCreatedCounter.Increase(orderRequest);

    await endpoint.Publish(new OrderCreatedEvent { Id = orderRequest.Id, Number = orderRequest.Number });
    
    logger.LogInformation("Published event for order {OrderNumber}...", orderRequest.Number);

    return Results.Created($"/orders/{orderRequest.Id}", orderRequest);
});

app.CreateDb();

app.Run();