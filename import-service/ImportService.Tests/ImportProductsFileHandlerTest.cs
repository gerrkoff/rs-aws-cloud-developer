using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using FakeItEasy;
using ImportService.Services;
using Xunit;

namespace ImportService.Tests;

public class ImportProductsFileHandlerTest
{
    [Fact]
    public async Task ImportProductsFile_ShouldReturnSignedUrl()
    {
        const string s3PreSignedUrl = "https://s3.amazonaws.com/.../products.csv";
        const string fileName = "products.csv";

        var importsService = A.Fake<IImportsService>();
        A.CallTo(() => importsService.GetSignedUrl(fileName)).Returns(s3PreSignedUrl);
        
        var context = new TestLambdaContext();
        var request = new APIGatewayProxyRequest
        {
            QueryStringParameters = new Dictionary<string, string>
            {
                ["name"] = fileName,
            }
        };
        
        var service = new ImportProductsFileHandler(importsService);
        var response = await service.Function(request, context);
        
        Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(s3PreSignedUrl, response.Body);
    }
    
    [Fact]
    public async Task ImportProductsFile_GivenNoName_ShouldReturn400()
    {
        var importsService = A.Fake<IImportsService>();
        var context = new TestLambdaContext();
        var request = new APIGatewayProxyRequest
        {
            QueryStringParameters = new Dictionary<string, string>()
        };
        
        var service = new ImportProductsFileHandler(importsService);
        var response = await service.Function(request, context);
        
        Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task ImportProductsFile_GivenNoQueryString_ShouldReturn500()
    {
        var importsService = A.Fake<IImportsService>();
        var context = new TestLambdaContext();
        var request = new APIGatewayProxyRequest();
        
        var service = new ImportProductsFileHandler(importsService);
        var response = await service.Function(request, context);
        
        Assert.Equal((int)HttpStatusCode.InternalServerError, response.StatusCode);
    }
    
    [Fact]
    public async Task ImportProductsFile_GivenFailingS3_ShouldReturn500()
    {
        var importsService = A.Fake<IImportsService>();
        A.CallTo(() => importsService.GetSignedUrl(A<string>._)).ThrowsAsync(new Exception());
        var context = new TestLambdaContext();
        var request = new APIGatewayProxyRequest
        {
            QueryStringParameters = new Dictionary<string, string>
            {
                ["name"] = "products.csv",
            }
        };
        
        var service = new ImportProductsFileHandler(importsService);
        var response = await service.Function(request, context);
        
        Assert.Equal((int)HttpStatusCode.InternalServerError, response.StatusCode);
    }
}
