using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.Apigatewayv2;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.SNS;
using Amazon.CDK.AWS.SNS.Subscriptions;
using Amazon.CDK.AWS.SQS;
using Amazon.CDK.AwsApigatewayv2Integrations;
using Constructs;
using AssetCode = Amazon.CDK.AWS.Lambda.AssetCode;
using HttpMethod = Amazon.CDK.AWS.Apigatewayv2.HttpMethod;
using SubscriptionFilter = Amazon.CDK.AWS.SNS.SubscriptionFilter;

namespace Deployment;

public class ProductServiceStack
{
    public IQueue CatalogItemsQueue { get; }
    
    public IFunction GetProductsFunction { get; }
    
    internal ProductServiceStack(Construct scope, IHttpRouteAuthorizer httpRouteAuthorizer)
    {
        var snsTopic = new Topic(scope, "CatalogItemCreatedTopic", new TopicProps
        {
            TopicName = "catalogItemCreatedTopic",
        });
        snsTopic.AddSubscription(new EmailSubscription("antonkofa@gmail.com"));
        snsTopic.AddSubscription(new EmailSubscription("ragnareck@mail.ru", new EmailSubscriptionProps
        {
            FilterPolicyWithMessageBody = new Dictionary<string, FilterOrPolicy>()
            {
                ["price"] = FilterOrPolicy.Filter(SubscriptionFilter.NumericFilter(new NumericConditions
                {
                    GreaterThan = 100499,
                })),
            },
        }));

        var productsTable = new Table(scope, "AwsShop.Products", new TableProps
        {
            PartitionKey = new Attribute { Name = "id", Type = AttributeType.STRING },
        });
        
        var stocksTable = new Table(scope, "AwsShop.Stocks", new TableProps
        {
            PartitionKey = new Attribute { Name = "productId", Type = AttributeType.STRING },
        });
        
        var lambdaEnvironment = new Dictionary<string, string>
        {
            ["TABLE_PRODUCTS"] = productsTable.TableName,
            ["TABLE_STOCKS"] = stocksTable.TableName,
            ["CATALOG_ITEM_CREATED_TOPIC"] = snsTopic.TopicArn,
        };
        
        var getProductsFunction = new Function(scope, "GetProductsLambda", new FunctionProps
        {
            Runtime = new Runtime("dotnet8"),
            Handler = "ProductService::ProductService.GetProductsHandler::Function",
            Code = new AssetCode("../dist/product-service"),
            LogRetention = RetentionDays.ONE_DAY,
            Environment = lambdaEnvironment,
            Timeout = Duration.Minutes(1),
        });

        var getProductByIdFunction = new Function(scope, "GetProductByIdLambda", new FunctionProps
        {
            Runtime = new Runtime("dotnet8"),
            Handler = "ProductService::ProductService.GetProductByIdHandler::Function",
            Code = new AssetCode("../dist/product-service"),
            LogRetention = RetentionDays.ONE_DAY,
            Environment = lambdaEnvironment,
            Timeout = Duration.Minutes(1),
        });
        
        var addProductFunction = new Function(scope, "AddProductLambda", new FunctionProps
        {
            Runtime = new Runtime("dotnet8"),
            Handler = "ProductService::ProductService.AddProductHandler::Function",
            Code = new AssetCode("../dist/product-service"),
            LogRetention = RetentionDays.ONE_DAY,
            Environment = lambdaEnvironment,
            Timeout = Duration.Minutes(1),
        });
        
        var catalogBatchProcessorFunction = new Function(scope, "CatalogBatchProcessorLambda", new FunctionProps
        {
            Runtime = new Runtime("dotnet8"),
            Handler = "ProductService::ProductService.CatalogBatchProcessor::Function",
            Code = new AssetCode("../dist/product-service"),
            LogRetention = RetentionDays.ONE_DAY,
            Environment = lambdaEnvironment,
            Timeout = Duration.Minutes(1),
        });

        productsTable.GrantFullAccess(getProductsFunction);
        productsTable.GrantFullAccess(getProductByIdFunction);
        productsTable.GrantFullAccess(addProductFunction);
        productsTable.GrantFullAccess(catalogBatchProcessorFunction);
        stocksTable.GrantFullAccess(getProductsFunction);
        stocksTable.GrantFullAccess(getProductByIdFunction);
        stocksTable.GrantFullAccess(addProductFunction);
        stocksTable.GrantFullAccess(catalogBatchProcessorFunction);
        
        CatalogItemsQueue = new Queue(scope, "CatalogBatchProcessorSQSQueue", new QueueProps
        {
            QueueName = "catalogItemsQueue",
            VisibilityTimeout = Duration.Seconds(90),
        });

        catalogBatchProcessorFunction.AddEventSource(new SqsEventSource(CatalogItemsQueue, new SqsEventSourceProps
        {
            BatchSize = 5,
        }));

        snsTopic.GrantPublish(catalogBatchProcessorFunction);

        var httpApi = new HttpApi(scope, "ProductServiceAPIGateway", new HttpApiProps
        {
            ApiName = "AwsShopProductService",
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
            Path = "/products",
            Methods = new[] { HttpMethod.GET },
            Integration = new HttpLambdaIntegration("GetProductsIntegration", getProductsFunction),
            Authorizer = httpRouteAuthorizer,
        });
        
        httpApi.AddRoutes(new AddRoutesOptions
        {
            Path = "/products",
            Methods = new[] { HttpMethod.POST },
            Integration = new HttpLambdaIntegration("AddProductIntegration", addProductFunction),
        });
        
        httpApi.AddRoutes(new AddRoutesOptions
        {
            Path = "/products/{id}",
            Methods = new[] { HttpMethod.GET },
            Integration = new HttpLambdaIntegration("GetProductByIdIntegration", getProductByIdFunction),
        });

        GetProductsFunction = getProductsFunction;
    }
}