using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using ProductService.Services;

namespace ProductService;

public class GetProductsHandler(IProductsService productsService) : HandlerBase
{
    public GetProductsHandler() : this(ServiceLocator.ProductsService)
    {
    }

    protected override async Task<APIGatewayProxyResponse> Handle(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var result = await productsService.GetProducts();

        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = JsonSerializer.Serialize(result, Helpers.JsonSerializerOptions),
        };
    }
}
