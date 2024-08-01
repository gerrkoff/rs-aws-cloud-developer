using CartService.Types;

namespace CartService.Entities;

public record CartEntity
{
    public required Guid Id { get; init; }
    
    public required string UserId { get; init; }
    
    public required DateTimeOffset CreatedAt { get; init; }
    
    public required DateTimeOffset UpdatedAt { get; init; }
    
    public required CartStatusType Status { get; init; }

    public required List<CartItemEntity> Items { get; init; }
    
    public class DbEntity
    {
        public required Guid Id { get; set; }
        
        public required string UserId { get; set; }
        
        public required DateTimeOffset CreatedAt { get; set; }
        
        public required DateTimeOffset UpdatedAt { get; set; }
        
        public required string Status { get; set; }
        
        public CartEntity ToEntity(List<CartItemEntity> items)
        {
            return new()
            {
                Id = Id,
                UserId = UserId,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt,
                Status = CartStatusType.FromString(Status),
                Items = items,
            };
        }
    }
}