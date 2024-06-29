using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.SQS;
using Common;
using ImportService.Services;

namespace ImportService;

public class ImportFileParser(IImportsService importsService, IAmazonSQS sqsClient, IEnvProvider envProvider)
{
    public ImportFileParser() : this(ServiceLocator.ImportsService, ServiceLocator.SqsClient, ServiceLocator.EnvProvider)
    {
    }

    public async Task Function(S3Event s3Event, ILambdaContext context)
    {
        foreach (var s3EventRecord in s3Event.Records)
        {
            var bucket = s3EventRecord.S3.Bucket.Name;
            var key = s3EventRecord.S3.Object.Key;
            context.Logger.LogInformation($"Parsing Bucket: {bucket} File: {key}");

            try
            {
                var products = (await importsService.ParseUploadedFile(bucket, key))
                    .Select(x => JsonSerializer.Serialize(x, Helpers.JsonSerializerOptions))
                    .ToList();

                foreach (var productString in products)
                {
                    await sqsClient.SendMessageAsync(envProvider.CatalogItemsQueue(), productString);
                }
                
                context.Logger.LogInformation($"Found {products.Count} in File: {s3EventRecord.S3.Object.Key}");
            }
            catch (Exception e)
            {
                context.Logger.LogError($"Error parsing file: {key}, exception: {e.Message}");
                continue;
            }

            await importsService.MoveToParsedFolder(bucket, key);
        }
    }
}
