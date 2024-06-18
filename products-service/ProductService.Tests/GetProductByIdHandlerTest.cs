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
    public void GetProductsById_GivenCorrectId_ShouldReturnProduct()
    {
        var product = new Product(Guid.NewGuid(), "Title", "Description", 10);
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
        var response = service.Function(request, context);
        
        Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(JsonSerializer.Serialize(product, Helpers.JsonSerializerOptions), response.Body);
    }
    
    [Fact]
    public void GetProductsById_GivenNoId_ShouldThrowException()
    {
        var productsService = A.Fake<IProductsService>();
        var context = new TestLambdaContext();
        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string>(),
        };
        
        var service = new GetProductByIdHandler(productsService);

        Assert.Throws<KeyNotFoundException>(() => service.Function(request, context));
    }
    
    [Fact]
    public void GetProductsById_GivenNotGuid_ShouldReturn400()
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
        var response = service.Function(request, context);
        
        Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(response.Body);
    }
    
    [Fact]
    public void GetProductsById_GivenNonExistentId_ShouldReturn404()
    {
        var id = Guid.NewGuid();
        var productsService = A.Fake<IProductsService>();
        A.CallTo(() => productsService.GetProductById(id)).Returns(null);
        var context = new TestLambdaContext();
        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string>
            {
                ["id"] = id.ToString(),
            },
        };
        
        var service = new GetProductByIdHandler(productsService);
        var response = service.Function(request, context);
        
        Assert.Equal((int)HttpStatusCode.NotFound, response.StatusCode);
        Assert.Null(response.Body);
    }
}