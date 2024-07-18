using System.Text;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Common;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AuthorizationService;

public class BasicAuthorizeHandler
{
    public APIGatewayCustomAuthorizerV2SimpleResponse Function(APIGatewayCustomAuthorizerV2Request request, ILambdaContext context)
    {
        var pathParamsString = JsonSerializer.Serialize(request.PathParameters, Helpers.JsonSerializerOptions);
        var queryParamsString = JsonSerializer.Serialize(request.QueryStringParameters, Helpers.JsonSerializerOptions);
        var headersString = JsonSerializer.Serialize(request.Headers, Helpers.JsonSerializerOptions);
        context.Logger.LogInformation($"{GetType().Name} request Path: {pathParamsString} Query: {queryParamsString} Headers: {headersString}");

        if (request.Headers == null
            || !request.Headers.TryGetValue("authorization", out var authHeader)
            || authHeader == null
            || !authHeader.StartsWith("Basic "))
        {
            context.Logger.LogInformation("No Authorization header found");
            return new APIGatewayCustomAuthorizerV2SimpleResponse
            {
                IsAuthorized = false,
            };
        }

        var encodedCredentials = authHeader.Substring("Basic ".Length);

        string credentials;
        try
        {
            credentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
        }
        catch (FormatException)
        {
            context.Logger.LogInformation("Invalid base64 credentials");
            return new APIGatewayCustomAuthorizerV2SimpleResponse
            {
                IsAuthorized = false,
            };
        }

        var parts = credentials.Split(':', 2);
        
        if (parts.Length != 2)
        {
            context.Logger.LogInformation("Invalid credentials format");
            return new APIGatewayCustomAuthorizerV2SimpleResponse
            {
                IsAuthorized = false,
            };
        }
        
        var username = parts[0];
        var password = parts[1];
        
        var expectedPassword = Environment.GetEnvironmentVariable($"USER_{username}");
        
        if (expectedPassword == null || expectedPassword != password)
        {
            context.Logger.LogInformation("Invalid credentials");
            return new APIGatewayCustomAuthorizerV2SimpleResponse
            {
                IsAuthorized = false,
            };
        }
        
        context.Logger.LogInformation("Authorized");
        return new APIGatewayCustomAuthorizerV2SimpleResponse
        {
            IsAuthorized = true,
        };
    }
}
