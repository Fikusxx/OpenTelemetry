namespace Orders.Models;

public sealed class Order
{
    public required Guid Id { get; init; }

    public required string Number { get; init; }
}