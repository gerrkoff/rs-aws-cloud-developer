using Amazon.DynamoDBv2.Model;
using Common.Models;

namespace ProductService.Services;

public interface IMapper
{
    Product MapProduct(Dictionary<string, AttributeValue> item);
    Dictionary<string, AttributeValue> MapProduct(Product product);
    Stock MapStock(Dictionary<string, AttributeValue> item);
    Dictionary<string, AttributeValue> MapStock(Stock stock);
    ProductWithStock CreateProductWithStock(Product product, Stock stock);
}

public class Mapper : IMapper
{
    public Product MapProduct(Dictionary<string, AttributeValue> item)
    {
        return new Product
        {
            Id = Guid.Parse(item["id"].S),
            Title = item["title"].S,
            Description = item["description"].S,
            Price = int.Parse(item["price"].N)
        };
    }

    public Dictionary<string, AttributeValue> MapProduct(Product product)
    {
        return new Dictionary<string, AttributeValue>
        {
            ["id"] = new() { S = product.Id.ToString() },
            ["title"] = new() { S = product.Title },
            ["description"] = new() { S = product.Description },
            ["price"] = new() { N = product.Price.ToString() }
        };
    }

    public Stock MapStock(Dictionary<string, AttributeValue> item)
    {
        return new Stock
        {
            ProductId = Guid.Parse(item["productId"].S),
            Count = int.Parse(item["count"].N)
        };
    }

    public Dictionary<string, AttributeValue> MapStock(Stock stock)
    {
        return new Dictionary<string, AttributeValue>
        {
            ["productId"] = new() { S = stock.ProductId.ToString() },
            ["count"] = new() { N = stock.Count.ToString() }
        };
    }

    public ProductWithStock CreateProductWithStock(Product product, Stock stock)
    {
        return new ProductWithStock
        {
            Id = product.Id,
            Title = product.Title,
            Description = product.Description,
            Price = product.Price,
            Count = stock.Count,
        };
    }
}