using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using ProductsService.Services;

namespace ProductsService;

public class GetProductListHandler
{
    private readonly IProductService _productService;

    /// <summary>
    /// Default constructor that Lambda will invoke.
    /// </summary>
    public GetProductListHandler(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// A Lambda function to respond to HTTP Get methods from API Gateway
    /// </summary>
    /// <remarks>
    /// This uses the <see href="https://github.com/aws/aws-lambda-dotnet/blob/master/Libraries/src/Amazon.Lambda.Annotations/README.md">Lambda Annotations</see> 
    /// programming model to bridge the gap between the Lambda programming model and a more idiomatic .NET model.
    /// 
    /// This automatically handles reading parameters from an APIGatewayProxyRequest
    /// as well as syncing the function definitions to serverless.template each time you build.
    /// 
    /// If you do not wish to use this model and need to manipulate the API Gateway 
    /// objects directly, see the accompanying Readme.md for instructions.
    /// </remarks>
    /// <param name="context">Information about the invocation, function, and execution environment</param>
    /// <returns>The response as an implicit <see cref="APIGatewayProxyResponse"/></returns>
    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Get, "/products")]
    public IHttpResult Get(ILambdaContext context)
    {
        context.Logger.LogInformation("Handling the 'GetProductsListHandler' Request");
        
        var result = _productService.GetProductsList();
        
        return HttpResults.Ok(result);
    }
}
