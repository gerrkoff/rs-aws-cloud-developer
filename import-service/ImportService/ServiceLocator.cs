using Amazon.Lambda.Core;
using Amazon.S3;
using ImportService.Services;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ImportService;

public static class ServiceLocator
{
    public static IImportsService ImportsService { get; }

    static ServiceLocator()
    {
        ImportsService = new ImportsService(new AmazonS3Client(), new EnvProvider());
    }
}
