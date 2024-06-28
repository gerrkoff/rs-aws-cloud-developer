using ProductService.Services;

namespace ProductService;

public static class ServiceLocator
{
    public static IProductsService ProductsService { get; }

    static ServiceLocator()
    {
        ProductsService = new ProductsService(new DbProvider(), new Mapper());
    }
}
