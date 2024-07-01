using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.Apigatewayv2;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AwsApigatewayv2Authorizers;
using Amazon.CDK.AwsApigatewayv2Integrations;
using Constructs;
using AssetCode = Amazon.CDK.AWS.Lambda.AssetCode;
using HttpMethod = Amazon.CDK.AWS.Apigatewayv2.HttpMethod;

namespace Deployment;

public class AuthorizationServiceStack
{
    internal AuthorizationServiceStack(Construct scope, IFunction importProductsFileFunction)
    {   
        var lambdaEnvironment = new Dictionary<string, string>
        {
            ["USER_gerrkoff"] = System.Environment.GetEnvironmentVariable("USER_gerrkoff"),
        };

        var basicAuthorizeFunction = new Function(scope, "BasicAuthorizeLambda", new FunctionProps
        {
            Runtime = new Runtime("dotnet8"),
            Handler = "AuthorizationService::AuthorizationService.BasicAuthorizeHandler::Function",
            Code = new AssetCode("../dist/authorization-service"),
            LogRetention = RetentionDays.ONE_DAY,
            Environment = lambdaEnvironment,
            Timeout = Duration.Minutes(1),
        });

        var basicLambdaAuthorizer = new HttpLambdaAuthorizer("ImportProductsFileAuthorizerTest", basicAuthorizeFunction,
            new HttpLambdaAuthorizerProps
            {
                IdentitySource = new[] { "$request.header.Authorization" },
                ResponseTypes = new[] { HttpLambdaResponseType.SIMPLE },
                ResultsCacheTtl = Duration.Minutes(0),
            });

        var httpApi = new HttpApi(scope, "TestAuthorizationServiceAPIGateway", new HttpApiProps
        {
            ApiName = "TestAwsShopAuthorizationService",
            CorsPreflight = new CorsPreflightOptions
            {
                AllowOrigins = new[] { "*" },
                AllowMethods = new[] { CorsHttpMethod.ANY },
                AllowHeaders = new[] { "*" },
                MaxAge = Duration.Hours(1),
            },
        });

        httpApi.AddRoutes(new AddRoutesOptions
        {
            Path = "/import",
            Methods = new[] { HttpMethod.GET },
            Integration = new HttpLambdaIntegration("ImportProductsFileIntegrationTest", importProductsFileFunction),
            Authorizer = basicLambdaAuthorizer,
        });
    }
}
