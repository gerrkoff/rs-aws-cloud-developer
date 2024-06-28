using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using ProductService.Services;

namespace ProductService;

public class GetProductByIdHandler(IProductsService productsService) : HandlerBase
{
    public GetProductByIdHandler() : this(ServiceLocator.ProductsService)
    {
    }

    protected override async Task<APIGatewayProxyResponse> Handle(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var id = request.PathParameters["id"];

        if (!Guid.TryParse(id, out var guid))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
            };
        }

        var result = await productsService.GetProductById(guid);

        if (result == null)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.NotFound,
            };
        }

        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = JsonSerializer.Serialize(result, Helpers.JsonSerializerOptions),
        };
    }
}
