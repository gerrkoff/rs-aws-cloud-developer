using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Constructs;

namespace Deployment;

public class BffServiceStack
{   
    internal BffServiceStack(Construct scope)
    {
        var restApi = new RestApi(scope, "BffServiceAPIGateway", new RestApiProps
        {
            RestApiName = "AwsShopBffService",
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
            DefaultIntegration = new HttpIntegration("http://bff-service-prod .eu-central-1.elasticbeanstalk.com/{proxy}", new HttpIntegrationProps
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
