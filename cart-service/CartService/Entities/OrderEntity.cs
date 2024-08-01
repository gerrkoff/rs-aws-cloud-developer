using System.Text.Json;
using CartService.Types;

namespace CartService.Entities;

public class OrderEntity
{
    public required Guid Id { get; init; }
    
    public required string UserId { get; init; }

    public required Guid CartId { get; init; }

    public required OrderEntityPayment Payment { get; init; }

    public required OrderEntityDelivery Delivery { get; init; }

    public required string Comments { get; init; }

    public required OrderStatusType Status { get; init; }

    public int Total { get; init; }

    public class DbEntity
    {
        public required Guid Id { get; init; }
        
        public required string UserId { get; init; }
        
        public required Guid CartId { get; init; }
        
        public required string Payment { get; init; }
        
        public required string Delivery { get; init; }
        
        public required string Comments { get; set; }
        
        public required string Status { get; set; }
        
        public required int Total { get; set; }
        
        public OrderEntity ToEntity()
        {
            return new()
            {
                Id = Id,
                UserId = UserId,
                CartId = CartId,
                Payment = JsonSerializer.Deserialize<OrderEntityPayment>(Payment) ?? throw new ArgumentNullException(nameof(Payment)),
                Delivery = JsonSerializer.Deserialize<OrderEntityDelivery>(Delivery) ?? throw new ArgumentNullException(nameof(Delivery)),
                Comments = Comments,
                Status = OrderStatusType.FromString(Status),
                Total = Total,
            };
        }
    }
    
    public record OrderEntityPayment
    {
        public required string Address { get; init; }
    }

    public record OrderEntityDelivery
    {
        public required string Address { get; init; }
    
        public required string FirstName { get; init; }
    
        public required string LastName { get; init; }
    }
}
