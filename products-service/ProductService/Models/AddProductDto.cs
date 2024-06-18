namespace ProductService.Models;

public class AddProductDto(string title, string description, int price)
{
    public string Title { get; private set; } = title;

    public string Description { get; private set; } = description;

    public int Price { get; private set; } = price;
}
