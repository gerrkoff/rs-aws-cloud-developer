using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.SimpleNotificationService;
using Common;
using Common.Models;
using ProductService.Services;

namespace ProductService;

public class CatalogBatchProcessor(IProductsService productsService, IAmazonSimpleNotificationService snsClient, IEnvProvider envProvider)
{
    public CatalogBatchProcessor() : this(ServiceLocator.ProductsService, ServiceLocator.SnsClient, ServiceLocator.EnvProvider)
    {
    }

    public async Task Function(SQSEvent sqsEvent, ILambdaContext context)
    {
        context.Logger.LogInformation($"Processing {sqsEvent.Records.Count} records");
        
        foreach (var message in sqsEvent.Records)
        {
            AddProductDto? addProduct;
            try
            {
                addProduct = JsonSerializer.Deserialize<AddProductDto>(message.Body, Helpers.JsonSerializerOptions);
            }
            catch (JsonException)
            {
                context.Logger.LogError($"Invalid message body: {message.Body}");
                continue;
            }

            if (!productsService.IsProductValid(addProduct))
            {
                context.Logger.LogError($"Invalid message body: {message.Body}");
                continue;
            }

            var product = await productsService.AddProduct(addProduct);
            var productString = JsonSerializer.Serialize(product, Helpers.JsonSerializerOptions);
            
            context.Logger.LogInformation($"Saved product: {productString}");

            await snsClient.PublishAsync(envProvider.CatalogItemCreatedTopic(), productString);
        }
    }
}
