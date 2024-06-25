namespace ProductService.Models;

public class AddProductDto(Guid? id, string title, string description, int price, int count)
{
    public Guid? Id { get; private set; } = id;

    public required string Title { get; init; } = title;

    public required string Description { get; init; } = description;

    public int Price { get; private set; } = price;
    
    public int Count { get; private set; } = count;
}
