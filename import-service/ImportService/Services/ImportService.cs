using System.Globalization;
using Amazon.S3;
using Amazon.S3.Model;
using Common.Models;
using CsvHelper;

namespace ImportService.Services;

public interface IImportsService
{
    Task<string> GetSignedUrl(string fileName);
    Task<IEnumerable<ProductWithStock>> ParseUploadedFile(string bucketName, string fileName);
    Task MoveToParsedFolder(string bucketName, string fileName);
}

public class ImportsService(
    IAmazonS3 s3Client,
    IEnvProvider envProvider) : IImportsService
{
    public async Task<string> GetSignedUrl(string fileName)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = envProvider.Bucket(),
            Key = $"uploaded/{fileName}",
            ContentType = "text/csv",
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.AddMinutes(1),
        };
        
        var signedUrl = await s3Client.GetPreSignedURLAsync(request);

        return signedUrl;
    }

    public async Task<IEnumerable<ProductWithStock>> ParseUploadedFile(string bucketName, string fileName)
    {
        var file = await s3Client.GetObjectAsync(bucketName, fileName);

        using var streamReader = new StreamReader(file.ResponseStream);
        using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);

        var records = new List<ProductWithStock>();
        await foreach (var record in csvReader.GetRecordsAsync<ProductWithStock>())
        {
            records.Add(record);
        }
        
        return records;
    }

    public async Task MoveToParsedFolder(string bucketName, string fileName)
    {
        await s3Client.CopyObjectAsync(new CopyObjectRequest
        {
            SourceBucket = bucketName,
            DestinationBucket = bucketName,
            SourceKey = fileName,
            DestinationKey = $"parsed/{fileName.Replace("uploaded/", "")}",
        });
        
        await s3Client.DeleteObjectAsync(bucketName, fileName);
    }
}
