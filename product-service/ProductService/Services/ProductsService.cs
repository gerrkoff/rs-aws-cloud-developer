using ProductService.Models;

namespace ProductService.Services;

public interface IProductsService
{
    List<Product> GetProductsList();
    Product? GetProductsById(Guid id);
}

public class ProductsService : IProductsService
{
    private readonly List<Product> _products = new()
    {
        new(Guid.Parse("7567ec4b-b10c-48c5-9345-fc73c48a80aa"), "Guitar", "Top guitar", 100),
        new(Guid.Parse("7567ec4b-b10c-48c5-9345-fc73c48a80a1"), "Drums", "Top drums", 200),
        new(Guid.Parse("7567ec4b-b10c-48c5-9345-fc73c48a80a3"), "Mic", "Top mic", 300),
        new(Guid.Parse("7567ec4b-b10c-48c5-9345-fc73348a80a2"), "Bass", "Top bass", 300)
    };
    
    public List<Product> GetProductsList()
    {
        return _products;
    }

    public Product? GetProductsById(Guid id)
    {   
        return _products.FirstOrDefault(p => p.Id == id);
    }
}
