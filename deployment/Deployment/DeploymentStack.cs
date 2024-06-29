using Amazon.CDK;
using Constructs;

namespace Deployment;

public class DeploymentStack : Stack
{
    internal DeploymentStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
    {
        var productServiceStack = new ProductServiceStack(this);
        _ = new ImportServiceStack(this, productServiceStack.CatalogItemsQueue);
    }
}
