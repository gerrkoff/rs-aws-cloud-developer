namespace CartService.Entities;

public record CartItemEntity
{
    public required Guid CartId { get; init; }
    
    public required Guid ProductId { get; init; }
    
    public required int Count { get; init; }
    
    public required int Price { get; init; }
}
