using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Common;
using ImportService.Services;

namespace ImportService;

public class ImportProductsFileHandler(IImportsService importsService) : ApiGatewayProxyHandlerBase
{
    public ImportProductsFileHandler() : this(ServiceLocator.ImportsService)
    {
    }

    protected override async Task<APIGatewayProxyResponse> Handle(APIGatewayProxyRequest request, ILambdaContext context)
    {
        if (request.QueryStringParameters == null
            || !request.QueryStringParameters.TryGetValue("name", out var fileName)
            || string.IsNullOrWhiteSpace(fileName))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
            };
        }

        var signedUrl = await importsService.GetSignedUrl(fileName);
        
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = signedUrl,
        };
    }
}
