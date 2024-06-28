using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Common;
using ImportService.Services;

namespace ImportService;

public class ImportFileParser(IImportsService importsService)
{
    public ImportFileParser() : this(ServiceLocator.ImportsService)
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
                var products = await importsService.ParseUploadedFile(bucket, key);

                foreach (var product in products)
                {
                    var productString = JsonSerializer.Serialize(product, Helpers.JsonSerializerOptions);
                    context.Logger.LogInformation(
                        $"Found in File: {s3EventRecord.S3.Object.Key} Product: {productString}");
                }
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
