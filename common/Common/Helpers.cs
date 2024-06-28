using System.Text.Json;

namespace Common;

public static class Helpers
{
    public static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
}