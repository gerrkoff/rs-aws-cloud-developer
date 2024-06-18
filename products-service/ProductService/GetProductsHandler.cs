using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using ProductService.Services;

namespace ProductService;

public class GetProductsHandler(IProductsService productsService)
{
    public GetProductsHandler() : this(ServiceLocator.ProductsService)
    {
    }

    public APIGatewayProxyResponse Function(APIGatewayProxyRequest request, ILambdaContext context)
    {
        context.Logger.LogInformation("GetProductsHandler request");
        
        var result = productsService.GetProducts();

        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = JsonSerializer.Serialize(result, Helpers.JsonSerializerOptions),
        };
    }
}
