using System.Text.Json.Serialization;

namespace ProductsService.Models;

public class Product(Guid id, string title, string description, int price)
{
    [JsonPropertyName("id")]
    public Guid Id { get; private set; } = id;

    [JsonPropertyName("title")]
    public string Title { get; private set; } = title;

    [JsonPropertyName("description")]
    public string Description { get; private set; } = description;

    [JsonPropertyName("price")]
    public int Price { get; private set; } = price;
}
