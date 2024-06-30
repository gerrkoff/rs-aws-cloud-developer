using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using ProductService.Services;
using Common;
using Common.Models;

namespace ProductService;

public class AddProductHandler(IProductsService productsService) : ApiGatewayProxyHandlerBase
{
    public AddProductHandler() : this(ServiceLocator.ProductsService)
    {
    }

    protected override async Task<APIGatewayProxyResponse> Handle(APIGatewayProxyRequest request, ILambdaContext context)
    {
        if (request.Body == null)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
            };
        }
        
        AddProductDto? model;
        try
        {
            model = JsonSerializer.Deserialize<AddProductDto>(request.Body, Helpers.JsonSerializerOptions);
        }
        catch (JsonException)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
            };
        }

        if (!productsService.IsProductValid(model))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
            };
        }

        await productsService.AddProduct(model);

        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
        };
    }
}
