using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Constructs;

namespace Deployment;

public class CartServiceStack
{   
    internal CartServiceStack(Construct scope)
    {
        var restApi = new RestApi(scope, "CartServiceAPIGateway3", new RestApiProps
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
            AnyMethod = true,
            DefaultMethodOptions = new MethodOptions
            {
                RequestParameters = new Dictionary<string, bool>
                {
                    { "method.request.path.proxy", true },
                }
            },
            DefaultIntegration = new HttpIntegration("http://gerrkoff-cart-api-prod2.eu-central-1.elasticbeanstalk.com/{proxy}", new HttpIntegrationProps
            {
                HttpMethod = "ANY",
                Proxy = true,
                Options = new IntegrationOptions
                {
                    RequestParameters = new Dictionary<string, string>
                    {
                        { "integration.request.path.proxy", "method.request.path.proxy" }
                    },
                },
            }),
        });
    }
}
