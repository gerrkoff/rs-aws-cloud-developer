using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace ProductService;

public abstract class HandlerBase
{
    public async Task<APIGatewayProxyResponse> Function(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var pathParamsString = JsonSerializer.Serialize(request.PathParameters, Helpers.JsonSerializerOptions);
        context.Logger.LogInformation($"{GetType().Name} request, PathParameters: {pathParamsString}, Body: {request.Body}");

        try
        {
            return await Handle(request, context);
        }
        catch (Exception e)
        {
            context.Logger.LogError($"{GetType().Name} error: {e.Message}");

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Body = JsonSerializer.Serialize(new { Message = "Something went wrong" },
                    Helpers.JsonSerializerOptions),
            };
        }
    }

    protected abstract Task<APIGatewayProxyResponse> Handle(APIGatewayProxyRequest request, ILambdaContext context);
}