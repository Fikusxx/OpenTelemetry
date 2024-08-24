namespace OrdersView.Models;

public sealed class Order
{
    public required Guid Id { get; set; }
    public required string Number { get; set; }
}