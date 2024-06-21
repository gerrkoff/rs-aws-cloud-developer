using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using FakeItEasy;
using ProductService.Models;
using ProductService.Services;
using Xunit;

namespace ProductService.Tests;

public class GetProductByIdHandlerTest
{
    [Fact]
    public async Task GetProductsById_GivenCorrectId_ShouldReturnProduct()
    {
        var product = new ProductWithStock(Guid.NewGuid(), "Title", "Description", 10, 1);
        var productsService = A.Fake<IProductsService>();
        A.CallTo(() => productsService.GetProductById(product.Id)).Returns(product);
        var context = new TestLambdaContext();
        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string>
            {
                ["id"] = product.Id.ToString(),
            },
        };
        
        var service = new GetProductByIdHandler(productsService);
        var response = await service.Function(request, context);
        
        Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(JsonSerializer.Serialize(product, Helpers.JsonSerializerOptions), response.Body);
    }
    
    [Fact]
    public async Task GetProductsById_GivenNoId_ShouldReturn500()
    {
        var productsService = A.Fake<IProductsService>();
        var context = new TestLambdaContext();
        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string>(),
        };
        
        var service = new GetProductByIdHandler(productsService);
        var response = await service.Function(request, context);

        Assert.Equal((int)HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(JsonSerializer.Serialize(new { Message = "Something went wrong" }, Helpers.JsonSerializerOptions), response.Body);
    }
    
    [Fact]
    public async Task GetProductsById_GivenNotGuid_ShouldReturn400()
    {
        var productsService = A.Fake<IProductsService>();
        var context = new TestLambdaContext();
        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string>
            {
                ["id"] = "someNotGuid",
            },
        };
        
        var service = new GetProductByIdHandler(productsService);
        var response = await service.Function(request, context);
        
        Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(response.Body);
    }
    
    [Fact]
    public async Task GetProductsById_GivenNonExistentId_ShouldReturn404()
    {
        var id = Guid.NewGuid();
        var productsService = A.Fake<IProductsService>();
        A.CallTo(() => productsService.GetProductById(id)).Returns(Task.FromResult<ProductWithStock?>(null));
        var context = new TestLambdaContext();
        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string>
            {
                ["id"] = id.ToString(),
            },
        };
        
        var service = new GetProductByIdHandler(productsService);
        var response = await service.Function(request, context);
        
        Assert.Equal((int)HttpStatusCode.NotFound, response.StatusCode);
        Assert.Null(response.Body);
    }
}