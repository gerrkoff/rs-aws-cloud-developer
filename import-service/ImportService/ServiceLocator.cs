using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.SQS;
using ImportService.Services;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ImportService;

public static class ServiceLocator
{
    public static IEnvProvider EnvProvider { get; } = new EnvProvider();

    public static IAmazonSQS SqsClient { get; } = new AmazonSQSClient();

    public static IImportsService ImportsService { get; } = new ImportsService(new AmazonS3Client(), EnvProvider);
}
