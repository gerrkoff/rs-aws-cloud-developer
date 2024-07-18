using Amazon.CDK;
using Constructs;

namespace Deployment;

public class DeploymentStack : Stack
{
    internal DeploymentStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
    {
        var authorizationServiceStack = new AuthorizationServiceStack(this);
        var productServiceStack = new ProductServiceStack(this, authorizationServiceStack.UserPoolAuthorizer);
        _ = new ImportServiceStack(this, productServiceStack.CatalogItemsQueue, authorizationServiceStack.BasicLambdaAuthorizer);
        
    }
}
