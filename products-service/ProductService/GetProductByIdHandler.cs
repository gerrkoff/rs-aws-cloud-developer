using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using ProductService.Services;

namespace ProductService;

public class GetProductByIdHandler(IProductsService productsService)
{
    public GetProductByIdHandler() : this(ServiceLocator.ProductsService)
    {
    }

    public APIGatewayProxyResponse Function(APIGatewayProxyRequest request, ILambdaContext context)
    {
        context.Logger.LogInformation("GetProductByIdHandler request");
        
        var id = request.PathParameters["id"];
        
        if (!Guid.TryParse(id, out var guid))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
            };
        }
        
        var result = productsService.GetProductById(guid);
        
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
