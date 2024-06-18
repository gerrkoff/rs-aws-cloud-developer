using Amazon.CDK.AWS.Apigatewayv2;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AwsApigatewayv2Integrations;
using Constructs;
using HttpMethod = Amazon.CDK.AWS.Apigatewayv2.HttpMethod;

namespace Deployment;

public class ProductServiceStack
{
    internal ProductServiceStack(Construct scope)
    {
        var getProductsFunction = new Function(scope, "GetProductsLambda", new FunctionProps
        {
            Runtime = new Runtime("dotnet8"),
            Handler = "ProductService::ProductService.GetProductsHandler::Function",
            Code = new AssetCode("../dist/product-service"),
        });

        var getProductByIdFunction = new Function(scope, "GetProductByIdLambda", new FunctionProps
        {
            Runtime = new Runtime("dotnet8"),
            Handler = "ProductService::ProductService.GetProductByIdHandler::Function",
            Code = new AssetCode("../dist/product-service"),
        });

        var httpApi = new HttpApi(scope, "ProductServiceAPIGateway", new HttpApiProps
        {
            ApiName = "AwsShopProductService",
            CorsPreflight = new CorsPreflightOptions
            {
                AllowOrigins = new[] { "*" },
                AllowMethods = new[] { CorsHttpMethod.ANY },
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
            Path = "/products/{id}",
            Methods = new[] { HttpMethod.GET },
            Integration = new HttpLambdaIntegration("GetProductByIdIntegration", getProductByIdFunction),
        });
    }
}