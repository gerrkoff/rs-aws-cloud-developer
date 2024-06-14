using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FakeItEasy;
using ProductService.Models;
using ProductService.Services;

namespace ProductService.Tests;

public class GetProductsListHandlerTests
{
    [Fact]
    public void GetProductsList_ShouldReturnProduct()
    {
        var productList = new List<Product>
        {
            new(Guid.NewGuid(), "Title 1", "Description 1", 10),
            new(Guid.NewGuid(), "Title 2", "Description 2", 20),
        };
        var productsService = A.Fake<IProductsService>();
        A.CallTo(() => productsService.GetProductsList()).Returns(productList);
        var request = new APIGatewayProxyRequest();
        
        var service = new GetProductsListHandler(productsService);
        var response = service.GetProductsList(request, A.Fake<ILambdaContext>());
        
        Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(JsonSerializer.Serialize(productList, Helpers.JsonSerializerOptions), response.Body);
    }
}