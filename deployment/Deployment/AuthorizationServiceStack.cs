using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.Apigatewayv2;
using Amazon.CDK.AWS.Cognito;
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
    internal AuthorizationServiceStack(Construct scope, IFunction importProductsFileFunction, IFunction getProductsFunction)
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
        
        var userPool = UserPool.FromUserPoolArn(scope, "UserPool", "arn:aws:cognito-idp:eu-central-1:399434948655:userpool/eu-central-1_KHhWnbE8V");
        
        httpApi.AddRoutes(new AddRoutesOptions
        {
            Path = "/products",
            Methods = new[] { HttpMethod.GET },
            Integration = new HttpLambdaIntegration("GetProductsFunctionIntegrationTest", getProductsFunction),
            Authorizer = new HttpUserPoolAuthorizer("UserPoolAuthorizing", userPool),
        });
    }
}
