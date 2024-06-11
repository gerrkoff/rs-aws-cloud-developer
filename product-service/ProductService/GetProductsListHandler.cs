using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using ProductService.Services;

namespace ProductService;

public class GetProductsListHandler
{
    private readonly IProductsService _productsService = new ProductsService();
    
    public APIGatewayProxyResponse GetProductsList(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var result = _productsService.GetProductsList();

        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = JsonSerializer.Serialize(result, Helpers.JsonSerializerOptions),
        };
    }
}
