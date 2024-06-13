using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using ProductsService.Services;

namespace ProductsService;

public class GetProductByIdHandler
{
    private readonly IProductService _productService;
    
    /// <summary>
    /// Default constructor that Lambda will invoke.
    /// </summary>
    public GetProductByIdHandler(IProductService productService)
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
    /// <param name="id"></param>
    /// <param name="context">Information about the invocation, function, and execution environment</param>
    /// <returns>The response as an implicit <see cref="APIGatewayProxyResponse"/></returns>
    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Get, "/products/{id}")]
    public IHttpResult Get(string id, ILambdaContext context)
    {
        context.Logger.LogInformation($"Handling the 'GetProductByIdHandler' Request, id={id}");
        
        if (!Guid.TryParse(id, out var guid))
            return HttpResults.BadRequest();
        
        var result = _productService.GetProductsById(guid);

        if (result == null)
            return HttpResults.NotFound();
        
        return HttpResults.Ok(result);
    }
}
