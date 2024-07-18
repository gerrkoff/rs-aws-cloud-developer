using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.Apigatewayv2;
using Amazon.CDK.AWS.Cognito;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AwsApigatewayv2Authorizers;
using Constructs;
using AssetCode = Amazon.CDK.AWS.Lambda.AssetCode;

namespace Deployment;

public class AuthorizationServiceStack
{
    public IHttpRouteAuthorizer BasicLambdaAuthorizer { get; }
    
    internal AuthorizationServiceStack(Construct scope)
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

        BasicLambdaAuthorizer = new HttpLambdaAuthorizer("ImportProductsFileAuthorizerTest", basicAuthorizeFunction,
            new HttpLambdaAuthorizerProps
            {
                IdentitySource = new[] { "$request.header.Authorization" },
                ResponseTypes = new[] { HttpLambdaResponseType.SIMPLE },
                ResultsCacheTtl = Duration.Minutes(0),
            });

        var userPool = UserPool.FromUserPoolArn(scope, "UserPool", "arn:aws:cognito-idp:eu-central-1:399434948655:userpool/eu-central-1_KHhWnbE8V");

        _ = new HttpUserPoolAuthorizer("UserPoolAuthorizing", userPool);
    }
}
