using System.Text.Json;
using Amazon.Lambda.Core;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ProductService;

public static class Helpers
{
    public static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
    
    public static string ToJsonString(this IDictionary<string, string> obj)
    {
        return JsonSerializer.Serialize(obj, JsonSerializerOptions);
    }
}