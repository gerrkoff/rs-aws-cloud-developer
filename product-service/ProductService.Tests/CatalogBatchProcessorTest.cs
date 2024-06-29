using Amazon.Lambda.SQSEvents;
using Amazon.Lambda.TestUtilities;
using Amazon.SimpleNotificationService;
using Common.Models;
using FakeItEasy;
using ProductService.Services;
using Xunit;

namespace ProductService.Tests;

public class CatalogBatchProcessorTest
{
    [Fact]
    public async Task CatalogBatchProcessor_GivenCorrectMessage_ShouldAddProduct()
    {        
        var productsService = A.Fake<IProductsService>();
        A.CallTo(() => productsService.IsProductValid(A<AddProductDto>._)).Returns(true);
        var snsClient = A.Fake<IAmazonSimpleNotificationService>();
        var envProvider = A.Fake<IEnvProvider>();
        A.CallTo(() => envProvider.CatalogItemCreatedTopic()).Returns("topic");
        var context = new TestLambdaContext();
        var request = new SQSEvent
        {
            Records = new List<SQSEvent.SQSMessage>
            {
                new()
                {
                    Body = """
                           {"title": "title", "description": "description", "price": 10, "count": 1}
                           """,
                },
            },
        };
        
        var service = new CatalogBatchProcessor(productsService, snsClient, envProvider);
        await service.Function(request, context);

        A.CallTo(() => productsService.AddProduct(A<AddProductDto>.That.Matches(
                x => x.Title == "title" && x.Description == "description" && x.Price == 10 && x.Count == 1)))
            .MustHaveHappened();
    }
    
    [Fact]
    public async Task CatalogBatchProcessor_GivenCorrectMessage_ShouldSendNotification()
    {        
        var productsService = A.Fake<IProductsService>();
        A.CallTo(() => productsService.IsProductValid(A<AddProductDto>._)).Returns(true);
        var snsClient = A.Fake<IAmazonSimpleNotificationService>();
        var envProvider = A.Fake<IEnvProvider>();
        A.CallTo(() => envProvider.CatalogItemCreatedTopic()).Returns("topic");
        var context = new TestLambdaContext();
        var request = new SQSEvent
        {
            Records = new List<SQSEvent.SQSMessage>
            {
                new()
                {
                    Body = """
                           {"title": "title", "description": "description", "price": 10, "count": 1}
                           """,
                },
            },
        };
        
        var service = new CatalogBatchProcessor(productsService, snsClient, envProvider);
        await service.Function(request, context);

        A.CallTo(() => snsClient.PublishAsync("topic", A<string>._, A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task CatalogBatchProcessor_GivenInvalidProduct_ShouldNotSendNotificationAndAddProduct()
    {
        var productsService = A.Fake<IProductsService>();
        A.CallTo(() => productsService.IsProductValid(A<AddProductDto>._)).Returns(false);
        var snsClient = A.Fake<IAmazonSimpleNotificationService>();
        var envProvider = A.Fake<IEnvProvider>();
        A.CallTo(() => envProvider.CatalogItemCreatedTopic()).Returns("topic");
        var context = new TestLambdaContext();
        var request = new SQSEvent
        {
            Records = new List<SQSEvent.SQSMessage>
            {
                new()
                {
                    Body = """
                           {"title": "title", "description": "description", "price": 10, "count": 1}
                           """,
                },
            },
        };
        
        var service = new CatalogBatchProcessor(productsService, snsClient, envProvider);
        await service.Function(request, context);

        A.CallTo(() => snsClient.PublishAsync(A<string>._, A<string>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => productsService.AddProduct(A<AddProductDto>._)).MustNotHaveHappened();
    }
    
    [Fact]
    public async Task CatalogBatchProcessor_GivenInvalidMessage_ShouldNotSendNotificationAndAddProduct()
    {
        var productsService = A.Fake<IProductsService>();
        A.CallTo(() => productsService.IsProductValid(A<AddProductDto>._)).Returns(true);
        var snsClient = A.Fake<IAmazonSimpleNotificationService>();
        var envProvider = A.Fake<IEnvProvider>();
        A.CallTo(() => envProvider.CatalogItemCreatedTopic()).Returns("topic");
        var context = new TestLambdaContext();
        var request = new SQSEvent
        {
            Records = new List<SQSEvent.SQSMessage>
            {
                new()
                {
                    Body = """
                           {"notTitle": "title", "description": "description", "price": 10, "count": 1}
                           """,
                },
            },
        };
        
        var service = new CatalogBatchProcessor(productsService, snsClient, envProvider);
        await service.Function(request, context);

        A.CallTo(() => snsClient.PublishAsync(A<string>._, A<string>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => productsService.AddProduct(A<AddProductDto>._)).MustNotHaveHappened();
    }
}