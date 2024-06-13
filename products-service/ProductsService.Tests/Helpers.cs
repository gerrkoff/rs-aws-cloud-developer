using System.Text.Json;
using Amazon.Lambda.Annotations.APIGateway;
using Xunit;

namespace ProductsService.Tests;

public static class Helpers
{
    public static void AssertBodyIsEqual<T>(T expectedBody, IHttpResult response)
    {
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        var expectedBodyJson = JsonSerializer.Serialize(expectedBody, options);
        
        var serializationOptions = new HttpResultSerializationOptions { Format = HttpResultSerializationOptions.ProtocolFormat.RestApi };
        var apiGatewayResponse = new StreamReader(response.Serialize(serializationOptions)).ReadToEnd();
        
        var responseJson = JsonSerializer.Deserialize<Response>(apiGatewayResponse, options);
        
        Assert.Equal(expectedBodyJson, responseJson?.Body);
    }
    
    private class Response
    {
        public required string Body { get; set; }
    }
}
