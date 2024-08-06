using Amazon.CDK;
using Constructs;

namespace Deployment;

public class DeploymentStack : Stack
{
    internal DeploymentStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
    {
        var authorizationServiceStack = new AuthorizationServiceStack(this);
        var productServiceStack = new ProductServiceStack(this);
        _ = new ImportServiceStack(this, productServiceStack.CatalogItemsQueue, authorizationServiceStack.BasicLambdaAuthorizer);
        _ = new CartServiceStack(this);
        _ = new BffServiceStack(this);
    }
}
