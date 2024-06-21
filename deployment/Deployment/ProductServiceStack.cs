using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.Apigatewayv2;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AwsApigatewayv2Integrations;
using Constructs;
using AssetCode = Amazon.CDK.AWS.Lambda.AssetCode;
using HttpMethod = Amazon.CDK.AWS.Apigatewayv2.HttpMethod;

namespace Deployment;

public class ProductServiceStack
{
    internal ProductServiceStack(Construct scope)
    {
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
        };
        
        var lambdaExecutionRole = new Role(scope, "LambdaExecutionRole", new RoleProps
        {
            AssumedBy = new ServicePrincipal("lambda.amazonaws.com"),
        });
        lambdaExecutionRole.AddManagedPolicy(ManagedPolicy.FromAwsManagedPolicyName("AmazonDynamoDBFullAccess"));
        lambdaExecutionRole.AddManagedPolicy(ManagedPolicy.FromAwsManagedPolicyName("service-role/AWSLambdaBasicExecutionRole"));
        lambdaExecutionRole.AddManagedPolicy(ManagedPolicy.FromAwsManagedPolicyName("service-role/AWSLambdaVPCAccessExecutionRole"));
        
        var getProductsFunction = new Function(scope, "GetProductsLambda", new FunctionProps
        {
            Runtime = new Runtime("dotnet8"),
            Handler = "ProductService::ProductService.GetProductsHandler::Function",
            Code = new AssetCode("../dist/product-service"),
            LogRetention = RetentionDays.ONE_DAY,
            Environment = lambdaEnvironment,
            Role = lambdaExecutionRole,
            Timeout = Duration.Minutes(1),
        });

        var getProductByIdFunction = new Function(scope, "GetProductByIdLambda", new FunctionProps
        {
            Runtime = new Runtime("dotnet8"),
            Handler = "ProductService::ProductService.GetProductByIdHandler::Function",
            Code = new AssetCode("../dist/product-service"),
            LogRetention = RetentionDays.ONE_DAY,
            Environment = lambdaEnvironment,
            Role = lambdaExecutionRole,
            Timeout = Duration.Minutes(1),
        });
        
        var addProductFunction = new Function(scope, "AddProductLambda", new FunctionProps
        {
            Runtime = new Runtime("dotnet8"),
            Handler = "ProductService::ProductService.AddProductHandler::Function",
            Code = new AssetCode("../dist/product-service"),
            LogRetention = RetentionDays.ONE_DAY,
            Environment = lambdaEnvironment,
            Role = lambdaExecutionRole,
            Timeout = Duration.Minutes(1),
        });

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
        });
        
        httpApi.AddRoutes(new AddRoutesOptions
        {
            Path = "/products",
            Methods = new[] { HttpMethod.PUT },
            Integration = new HttpLambdaIntegration("AddProductIntegration", addProductFunction),
        });
        
        httpApi.AddRoutes(new AddRoutesOptions
        {
            Path = "/products/{id}",
            Methods = new[] { HttpMethod.GET },
            Integration = new HttpLambdaIntegration("GetProductByIdIntegration", getProductByIdFunction),
        });
    }
}