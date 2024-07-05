namespace CartService.Dto;

public class AddOrderDto
{
    public required AddOrderDtoAddress Address { get; set; }
    
    public record AddOrderDtoAddress
    {
        public required string Address { get; set; }
    
        public required string Comment { get; set; }
    
        public required string FirstName { get; set; }
    
        public required string LastName { get; set; }
    }
}
