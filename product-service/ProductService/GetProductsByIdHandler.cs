using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using ProductService.Services;

namespace ProductService;

public class GetProductsByIdHandler
{
    private readonly IProductsService _productsService;

    public GetProductsByIdHandler()
    {
        _productsService = new ProductsService();
    }
    
    public GetProductsByIdHandler(IProductsService productsService)
    {
        _productsService = productsService;
    }
    
    public APIGatewayProxyResponse GetProductsById(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var id = request.PathParameters["id"];
        
        if (!Guid.TryParse(id, out var guid))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
            };
        }
        
        var result = _productsService.GetProductsById(guid);
        
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
