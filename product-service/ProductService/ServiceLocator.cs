using Amazon.Lambda.Core;
using Amazon.SimpleNotificationService;
using ProductService.Services;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ProductService;

public static class ServiceLocator
{
    public static IEnvProvider EnvProvider { get; } = new EnvProvider();

    public static IAmazonSimpleNotificationService SnsClient { get; } = new AmazonSimpleNotificationServiceClient();
    
    public static IProductsService ProductsService { get; } = new ProductsService(EnvProvider, new Mapper());
}
