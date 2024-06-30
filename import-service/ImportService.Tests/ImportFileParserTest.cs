using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.Lambda.TestUtilities;
using Amazon.SQS;
using Common;
using Common.Models;
using FakeItEasy;
using ImportService.Services;
using Xunit;

namespace ImportService.Tests;

public class ImportFileParserTest
{
    [Fact]
    public async Task ImportFile_ShouldSendFoundProductsToQueue()
    {
        const string fileName = "products.csv";
        const string bucketName = "bucket";
        List<ProductWithStock> products =
        [
            new ProductWithStock
            {
                Id = Guid.Parse("60e6d354-f03c-4fcf-a6a3-0b6b26357b3e"),
                Title = "Title 1",
                Description = "Description 1",
                Price = 10,
                Count = 1,
            },
            new ProductWithStock
            {
                Id = Guid.Parse("e106e7ca-9b33-4b7f-bdbe-56f4ac9c5a14"),
                Title = "Title 2",
                Description = "Description 2",
                Price = 20,
                Count = 2,
            },
        ];
        var importService = A.Fake<IImportsService>();
        A.CallTo(() => importService.ParseUploadedFile(bucketName, fileName)).Returns(products);
        var envProvider = A.Fake<IEnvProvider>();
        A.CallTo(() => envProvider.CatalogItemsQueue()).Returns("queue");
        var sqsClient = A.Fake<IAmazonSQS>();
        var logger = A.Fake<ILambdaLogger>();
        var context = new TestLambdaContext { Logger = logger };
        var request = CreateS3Event(bucketName, fileName);
        
        var service = new ImportFileParser(importService, sqsClient, envProvider);
        await service.Function(request, context);

        foreach (var product in products)
        {
            var productJson = JsonSerializer.Serialize(product, Helpers.JsonSerializerOptions);
            A.CallTo(() => sqsClient.SendMessageAsync("queue", productJson, A<CancellationToken>._))
                .MustHaveHappened();
        }
    }

    [Fact]
    public async Task ImportFile_ShouldNotFailIfParsingFileFails()
    {
        const string fileName = "products.csv";
        var importService = A.Fake<IImportsService>();
        A.CallTo(() => importService.ParseUploadedFile(A<string>._, A<string>._))
            .ThrowsAsync(new Exception());
        var logger = A.Fake<ILambdaLogger>();
        var context = new TestLambdaContext { Logger = logger };
        var request = CreateS3Event(A.Dummy<string>(), fileName);
        
        var service = new ImportFileParser(importService, A.Fake<IAmazonSQS>(), A.Fake<IEnvProvider>());
        await service.Function(request, context);

        A.CallTo(() => logger.LogError(A<string>.That.Contains(fileName)))
            .MustHaveHappened();
    }

    [Fact]
    public async Task ImportFile_ShouldMoveFile()
    {
        const string fileName = "products.csv";
        const string bucketName = "bucket";
        var importService = A.Fake<IImportsService>();
        var context = new TestLambdaContext();
        var request = CreateS3Event(bucketName, fileName);
        
        var service = new ImportFileParser(importService, A.Fake<IAmazonSQS>(), A.Fake<IEnvProvider>());
        await service.Function(request, context);

        A.CallTo(() => importService.MoveToParsedFolder(bucketName, fileName))
            .MustHaveHappened();
    }

    private static S3Event CreateS3Event(string bucketName, string fileName) =>
        new()
        {
            Records =
            [
                new()
                {
                    S3 = new S3Event.S3Entity
                    {
                        Bucket = new S3Event.S3BucketEntity
                        {
                            Name = bucketName,
                        },
                        Object = new S3Event.S3ObjectEntity
                        {
                            Key = fileName,
                        },
                    },
                }

            ],
        };
}
