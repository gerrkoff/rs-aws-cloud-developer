namespace CartService.Dto;

public class CartItemDto
{
    public required CartItemDtoProduct Product { get; init; }
    
    public required int Count { get; init; }

    public class CartItemDtoProduct
    {
        public required Guid Id { get; init; } = Guid.Empty;
        
        public required int Price { get; init; }
    }
}
