namespace Common.Models;

public class Product
{
    public Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Description { get; init; }

    public int Price { get; init; }
}
