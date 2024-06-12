using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FakeItEasy;
using ProductService.Models;
using ProductService.Services;

namespace ProductService.Tests;

public class GetProductsByIdHandlerTests
{
    [Fact]
    public void GetProductsById_GivenCorrectId_ShouldReturnProduct()
    {
        var product = new Product(Guid.NewGuid(), "Title", "Description", 10);
        var productsService = A.Fake<IProductsService>();
        A.CallTo(() => productsService.GetProductsById(product.Id)).Returns(product);
        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string>
            {
                ["id"] = product.Id.ToString(),
            },
        };
        
        var service = new GetProductsByIdHandler(productsService);
        var response = service.GetProductsById(request, A.Fake<ILambdaContext>());
        
        Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(JsonSerializer.Serialize(product, Helpers.JsonSerializerOptions), response.Body);
    }
    
    [Fact]
    public void GetProductsById_GivenNoId_ShouldThrowAnException()
    {
        var productsService = A.Fake<IProductsService>();
        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string>(),
        };
        
        var service = new GetProductsByIdHandler(productsService);

        Assert.Throws<KeyNotFoundException>(() => service.GetProductsById(request, A.Fake<ILambdaContext>()));
    }
    
    [Fact]
    public void GetProductsById_GivenNotGuid_ShouldReturn400()
    {
        var productsService = A.Fake<IProductsService>();
        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string>
            {
                ["id"] = "someNotGuid",
            },
        };
        
        var service = new GetProductsByIdHandler(productsService);
        var response = service.GetProductsById(request, A.Fake<ILambdaContext>());
        
        Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(response.Body);
    }
    
    [Fact]
    public void GetProductsById_GivenNonExistentId_ShouldReturn404()
    {
        var id = Guid.NewGuid();
        var productsService = A.Fake<IProductsService>();
        A.CallTo(() => productsService.GetProductsById(id)).Returns(null);
        
        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string>
            {
                ["id"] = id.ToString(),
            },
        };
        
        var service = new GetProductsByIdHandler(productsService);
        var response = service.GetProductsById(request, A.Fake<ILambdaContext>());
        
        Assert.Equal((int)HttpStatusCode.NotFound, response.StatusCode);
        Assert.Null(response.Body);
    }
}