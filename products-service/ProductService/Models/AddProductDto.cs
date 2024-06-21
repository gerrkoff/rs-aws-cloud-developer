namespace ProductService.Models;

public class AddProductDto(Guid? id, string title, string description, int price, int count)
{
    public Guid? Id { get; private set; } = id;
    public string Title { get; private set; } = title;

    public string Description { get; private set; } = description;

    public int Price { get; private set; } = price;
    
    public int Count { get; private set; } = count;
}
