namespace OrderContracts;

public sealed record OrderCreatedEvent
{
    public required Guid Id { get; set; }
    public required string Number { get; set; }
}