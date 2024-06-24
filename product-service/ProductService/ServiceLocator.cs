using Amazon.Lambda.Core;
using ProductService.Services;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ProductService;

public static class ServiceLocator
{
    public static IProductsService ProductsService { get; }

    static ServiceLocator()
    {
        ProductsService = new ProductsService(new DbProvider(), new Mapper());
    }
}
