using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Logs;
using Constructs;
using AssetCode = Amazon.CDK.AWS.Lambda.AssetCode;

namespace Deployment;

public class CartServiceStack
{   
    internal CartServiceStack(Construct scope)
    {   
        var lambdaEnvironment = new Dictionary<string, string>
        {
            ["RDS_CONNECTION_STRING"] = System.Environment.GetEnvironmentVariable("RDS_CONNECTION_STRING"),
        };
        
        var cartServiceFunction = new Function(scope, "CartServiceLambda", new FunctionProps
        {
            Runtime = new Runtime("dotnet8"),
            Handler = "CartService::CartService.LambdaEntryPoint::FunctionHandlerAsync",
            Code = new AssetCode("../dist/cart-service"),
            LogRetention = RetentionDays.ONE_DAY,
            Environment = lambdaEnvironment,
            Timeout = Duration.Minutes(1),
        });

        var restApi = new RestApi(scope, "CartServiceAPIGateway2", new RestApiProps
        {
            RestApiName = "AwsShopCartService",
            DefaultCorsPreflightOptions = new CorsOptions
            {
                AllowOrigins = new[] { "*" },
                AllowMethods = new[] { "*" },
                AllowHeaders = new[] { "*" },
                MaxAge = Duration.Hours(1),
            },
        });

        restApi.Root.AddProxy(new ProxyResourceOptions
        {
            DefaultIntegration = new LambdaIntegration(cartServiceFunction),
        });
    }
}