using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
using FakeItEasy;
using ImportService.Services;
using Xunit;

namespace ImportService.Tests;

public class ImportServiceTest
{
    [Fact]
    public async Task GetSignedUrl_ShouldReturnSignedUrl()
    {
        const string s3PreSignedUrl = "https://s3.amazonaws.com/.../products.csv";
        const string fileName = "products.csv";
        const string bucketName = "bucket";

        var amazonS3Client = A.Fake<IAmazonS3>();
        A.CallTo(() => amazonS3Client.GetPreSignedURLAsync(A<GetPreSignedUrlRequest>.That
                .Matches(r =>
                    r.Key == $"uploaded/{fileName}" &&
                    r.BucketName == bucketName)))
            .Returns(Task.FromResult(s3PreSignedUrl));

        var envProvider = A.Fake<IEnvProvider>();
        A.CallTo(() => envProvider.Bucket()).Returns(bucketName);

        var importsService = new ImportsService(amazonS3Client, envProvider);
        
        var signedUrl = await importsService.GetSignedUrl(fileName);
        
        Assert.Equal(s3PreSignedUrl, signedUrl);
    }

    [Fact]
    public async Task ParseUploadedFile_ShouldReturnProducts()
    {
        const string fileName = "products.csv";
        const string bucketName = "bucket";
        var fileStreamContent = """
                                Id,Title,Description,Price,Count
                                60e6d354-f03c-4fcf-a6a3-0b6b26357b3e,Title 1,Description 1,10,1
                                e106e7ca-9b33-4b7f-bdbe-56f4ac9c5a14,Title 2,Description 2,20,2
                                """;
        var fileMock = new GetObjectResponse
        {
            ResponseStream = new MemoryStream(Encoding.UTF8.GetBytes(fileStreamContent)),
        };
        var amazonS3Client = A.Fake<IAmazonS3>();
        A.CallTo(() => amazonS3Client.GetObjectAsync(bucketName, fileName, A<CancellationToken>._))
            .Returns(Task.FromResult(fileMock));
        var envProvider = A.Fake<IEnvProvider>();
        
        var importsService = new ImportsService(amazonS3Client, envProvider);
        var parsedProducts = await importsService.ParseUploadedFile(bucketName, fileName);
        
        Assert.Collection(parsedProducts,
            x =>
            {
                Assert.Equal(Guid.Parse("60e6d354-f03c-4fcf-a6a3-0b6b26357b3e"), x.Id);
                Assert.Equal("Title 1", x.Title);
                Assert.Equal("Description 1", x.Description);
                Assert.Equal(10, x.Price);
                Assert.Equal(1, x.Count);
            },
            x =>
            {
                Assert.Equal(Guid.Parse("e106e7ca-9b33-4b7f-bdbe-56f4ac9c5a14"), x.Id);
                Assert.Equal("Title 2", x.Title);
                Assert.Equal("Description 2", x.Description);
                Assert.Equal(20, x.Price);
                Assert.Equal(2, x.Count);
            });
    }

    [Fact]
    public async Task MoveToParsedFolder_ShouldCopyAndDeleteFile()
    {
        const string fileName = "uploaded/products.csv";
        const string bucketName = "bucket";
        var amazonS3Client = A.Fake<IAmazonS3>();
        var envProvider = A.Fake<IEnvProvider>();
        
        var importsService = new ImportsService(amazonS3Client, envProvider);
        await importsService.MoveToParsedFolder(bucketName, fileName);

        A.CallTo(() => amazonS3Client.CopyObjectAsync(A<CopyObjectRequest>.That
                .Matches(r =>
                    r.SourceBucket == bucketName &&
                    r.DestinationBucket == bucketName &&
                    r.SourceKey == fileName &&
                    r.DestinationKey == "parsed/products.csv"), A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => amazonS3Client.DeleteObjectAsync(bucketName, fileName, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}