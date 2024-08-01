namespace CartService.Dto;

public record OrderDto
{
    public required Guid Id { get; init; }
    
    public required string UserId { get; init; }

    public required Guid CartId { get; init; }

    public required List<CartItemDto> Items { get; init; }

    public required OrderDtoAddress Address { get; init; }

    public required string Comments { get; init; }

    public required string Status { get; init; }

    public int Total { get; init; }
    
    public required List<OrderDtoStatus> StatusHistory { get; init; }
    
    public class OrderDtoAddress
    {
        public required string Address { get; init; }
    
        public required string FirstName { get; init; }
    
        public required string LastName { get; init; }
    }

    public class OrderDtoStatus
    {
        public required string Status { get; init; }
    }
}
