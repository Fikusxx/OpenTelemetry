using MassTransit;
using Orders.Contracts;
using OrdersView.Database;
using OrdersView.Models;

namespace OrdersView.Consumers;

public sealed class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly OrdersDbContext dbContext;

    public OrderCreatedEventConsumer(OrdersDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var order = new Order { Id = context.Message.Id, Number = context.Message.Number };
        await dbContext.Orders.AddAsync(order);
        await dbContext.SaveChangesAsync();
    }
}