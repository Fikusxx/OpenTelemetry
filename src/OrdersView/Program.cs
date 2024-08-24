using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using OrdersView;
using OrdersView.Database;
using OrdersView.Models;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.AddDatabase();
builder.AddObservability();
builder.AddTransport();
builder.AddRedis();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.MapGet("/", () => "Hello");

app.MapGet("/orders/{id:guid}", async (
    [FromRoute] Guid id,
    [FromServices] OrdersDbContext dbContext,
    [FromServices] IDistributedCache cache) =>
{
    var cacheKey = $"order-{id}";
    var orderJson = await cache.GetStringAsync(cacheKey);

    if (string.IsNullOrWhiteSpace(orderJson) == false)
    {
        return Results.Ok(JsonSerializer.Deserialize<Order>(orderJson));
    }

    var order = await dbContext.Orders.FindAsync(id);

    if (order is null)
        return Results.NotFound(":(");

    orderJson = JsonSerializer.Serialize(order);
    await cache.SetStringAsync(cacheKey, orderJson, new DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
    });

    return Results.Ok(order);
});


app.CreateDb();

app.Run();