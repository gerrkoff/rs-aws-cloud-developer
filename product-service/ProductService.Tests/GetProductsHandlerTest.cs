using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Xunit;
using Amazon.Lambda.TestUtilities;
using Common;
using Common.Models;
using FakeItEasy;
using ProductService.Services;

namespace ProductService.Tests;

public class GetProductsHandlerTest
{
    [Fact]
    public async Task GetProductsList_ShouldReturnProduct()
    {
        var productList = new List<ProductWithStock>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Title 1",
                Description = "Description 1",
                Price = 10,
                Count = 1,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Title 2",
                Description = "Description 2",
                Price = 20,
                Count = 2,
            },
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
