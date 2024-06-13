using System.Net;
using Amazon.Lambda.TestUtilities;
using FakeItEasy;
using ProductsService.Models;
using ProductsService.Services;
using Xunit;

namespace ProductsService.Tests;

public class GetProductListHandlerTests
{
    [Fact]
    public void Get_ShouldReturnProduct()
    {
        var productList = new List<Product>
        {
            new(Guid.NewGuid(), "Title 1", "Description 1", 10),
            new(Guid.NewGuid(), "Title 2", "Description 2", 20),
        };
        var productsService = A.Fake<IProductService>();
        A.CallTo(() => productsService.GetProductsList()).Returns(productList);
        var context = new TestLambdaContext();
        
        var service = new GetProductListHandler(productsService);
        var response = service.Get(context);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Helpers.AssertBodyIsEqual(productList, response);
    }
}