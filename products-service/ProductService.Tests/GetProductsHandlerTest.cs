using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Xunit;
using Amazon.Lambda.TestUtilities;
using FakeItEasy;
using ProductService.Models;
using ProductService.Services;

namespace ProductService.Tests;

public class GetProductsHandlerTest
{
    [Fact]
    public async Task GetProductsList_ShouldReturnProduct()
    {
        var productList = new List<ProductWithStock>
        {
            new(Guid.NewGuid(), "Title 1", "Description 1", 10, 1),
            new(Guid.NewGuid(), "Title 2", "Description 2", 20, 2),
        };
        var productsService = A.Fake<IProductsService>();
        A.CallTo(() => productsService.GetProducts()).Returns(Task.FromResult(productList));
        var context = new TestLambdaContext();
        var request = new APIGatewayProxyRequest();
        
        var service = new GetProductsHandler(productsService);
        var response = await service.Function(request, context);
        
        Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(JsonSerializer.Serialize(productList, Helpers.JsonSerializerOptions), response.Body);
    }
}
