using System.Net;
using Amazon.Lambda.TestUtilities;
using FakeItEasy;
using ProductsService.Models;
using ProductsService.Services;
using Xunit;

namespace ProductsService.Tests;

public class GetProductByIdHandlerTests
{
    [Fact]
    public void Get_GivenCorrectId_ShouldReturnProduct()
    {
        var product = new Product(Guid.NewGuid(), "Title", "Description", 10);
        var productsService = A.Fake<IProductService>();
        A.CallTo(() => productsService.GetProductsById(product.Id)).Returns(product);
        var context = new TestLambdaContext();
        
        var service = new GetProductByIdHandler(productsService);
        var response = service.Get(product.Id.ToString(), context);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Helpers.AssertBodyIsEqual(product, response);
    }
    
    [Fact]
    public void Get_GivenNotGuid_ShouldReturn400()
    {
        var productsService = A.Fake<IProductService>();
        var context = new TestLambdaContext();
        
        var service = new GetProductByIdHandler(productsService);
        var response = service.Get("someNotGuid", context);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public void Get_GivenNonExistentId_ShouldReturn404()
    {
        var id = Guid.NewGuid();
        var productsService = A.Fake<IProductService>();
        A.CallTo(() => productsService.GetProductsById(id)).Returns(null);
        var context = new TestLambdaContext();
        
        var service = new GetProductByIdHandler(productsService);
        var response = service.Get(id.ToString(), context);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}