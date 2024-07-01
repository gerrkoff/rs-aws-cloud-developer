using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.Apigatewayv2;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.S3.Notifications;
using Amazon.CDK.AWS.SQS;
using Amazon.CDK.AwsApigatewayv2Integrations;
using Constructs;
using AssetCode = Amazon.CDK.AWS.Lambda.AssetCode;
using HttpMethod = Amazon.CDK.AWS.Apigatewayv2.HttpMethod;

namespace Deployment;

public class ImportServiceStack
{
    public IFunction ImportProductsFileFunction { get; }

    internal ImportServiceStack(Construct scope, IQueue catalogItemsQueue)
    {
        var s3Bucket = new Bucket(scope, "AwsShopImportProductsBucket", new BucketProps
        {
            RemovalPolicy = RemovalPolicy.DESTROY,
            BlockPublicAccess = BlockPublicAccess.BLOCK_ALL,
            AutoDeleteObjects = true,
            Cors = new ICorsRule[]
            {
                new CorsRule
                {
                    AllowedOrigins = new[] { "*" },
                    AllowedMethods = new[] { HttpMethods.PUT },
                    AllowedHeaders = new[] { "*" },
                    MaxAge = 60,
                }
            }
        });
        
        var lambdaEnvironment = new Dictionary<string, string>
        {
            ["IMPORT_BUCKET"] = s3Bucket.BucketName,
            ["CATALOG_ITEMS_QUEUE"] = catalogItemsQueue.QueueName,
        };

        ImportProductsFileFunction = new Function(scope, "ImportProductsFileLambda", new FunctionProps
        {
            Runtime = new Runtime("dotnet8"),
            Handler = "ImportService::ImportService.ImportProductsFileHandler::Function",
            Code = new AssetCode("../dist/import-service"),
            LogRetention = RetentionDays.ONE_DAY,
            Environment = lambdaEnvironment,
            Timeout = Duration.Minutes(1),
        });
        
        var importFileParserFunction = new Function(scope, "ImportFileParserLambda", new FunctionProps
        {
            Runtime = new Runtime("dotnet8"),
            Handler = "ImportService::ImportService.ImportFileParser::Function",
            Code = new AssetCode("../dist/import-service"),
            LogRetention = RetentionDays.ONE_DAY,
            Environment = lambdaEnvironment,
            Timeout = Duration.Minutes(1),
        });

        s3Bucket.AddEventNotification(EventType.OBJECT_CREATED,
            new LambdaDestination(importFileParserFunction),
            new NotificationKeyFilter
            {
                Prefix = "uploaded/"
            });
        s3Bucket.GrantPut(ImportProductsFileFunction);
        s3Bucket.GrantReadWrite(importFileParserFunction);
        catalogItemsQueue.GrantSendMessages(importFileParserFunction);

        var httpApi = new HttpApi(scope, "ImportServiceAPIGateway", new HttpApiProps
        {
            ApiName = "AwsShopImportService",
            CorsPreflight = new CorsPreflightOptions
            {
                AllowOrigins = new[] { "*" },
                AllowMethods = new[] { CorsHttpMethod.ANY },
                AllowHeaders = new[] { "*" },
                MaxAge = Duration.Hours(1),
            }
        });

        httpApi.AddRoutes(new AddRoutesOptions
        {
            Path = "/import",
            Methods = new[] { HttpMethod.GET },
            Integration = new HttpLambdaIntegration("ImportProductsFileIntegration", ImportProductsFileFunction),
        });
    }
}