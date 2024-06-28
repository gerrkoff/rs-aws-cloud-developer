using Amazon.DynamoDBv2.Model;
using ProductService.Models;

namespace ProductService.Services;

public interface IProductsService
{
    Task<List<ProductWithStock>> GetProducts();
    Task<ProductWithStock?> GetProductById(Guid id);
    Task AddProduct(AddProductDto addProduct);
}

public class ProductsService(
    IDbProvider dbProvider,
    IMapper mapper) : IProductsService
{
    public async Task<List<ProductWithStock>> GetProducts()
    {
        var productsWithStock = new List<ProductWithStock>();
        
        var productsResponse = await dbProvider.Client().ScanAsync(new ScanRequest
        {
            TableName = dbProvider.ProductsTable(),
        });

        var products = productsResponse.Items
            .Select(mapper.MapProduct)
            .ToDictionary(x => x.Id, x => x);

        foreach (var keysChunk in products.Keys.Chunk(100))
        {
            var stocksResponse = await dbProvider.Client().BatchGetItemAsync(new BatchGetItemRequest
            {
                RequestItems = new Dictionary<string, KeysAndAttributes>
                {
                    [dbProvider.StocksTable()] = new()
                    {
                        Keys = keysChunk.Select(id => new Dictionary<string, AttributeValue>
                        {
                            ["productId"] = new() { S = id.ToString() }
                        }).ToList()
                    },
                },
            });
            
            foreach (var item in stocksResponse.Responses[dbProvider.StocksTable()])
            {
                var stock = mapper.MapStock(item);
                var product = products[stock.ProductId];
                productsWithStock.Add(mapper.CreateProductWithStock(product, stock));
            }
        }

        return productsWithStock;
    }

    public async Task<ProductWithStock?> GetProductById(Guid id)
    {
        var productItemTask = dbProvider.Client().QueryAsync(new QueryRequest
        {
            TableName = dbProvider.ProductsTable(),
            KeyConditionExpression = "id = :id",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":id"] = new() { S = id.ToString() }
            }
        });
        
        var stockItemTask = dbProvider.Client().QueryAsync(new QueryRequest
        {
            TableName = dbProvider.StocksTable(),
            KeyConditionExpression = "productId = :id",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":id"] = new() { S = id.ToString() }
            }
        });

        await Task.WhenAll(productItemTask, stockItemTask);

        var product = productItemTask.Result.Items.Select(mapper.MapProduct).SingleOrDefault();
        var stock = stockItemTask.Result.Items.Select(mapper.MapStock).SingleOrDefault();
        
        return product == null || stock == null
            ? null
            : mapper.CreateProductWithStock(product, stock);
    }

    public async Task AddProduct(AddProductDto addProduct)
    {
        var id = addProduct.Id ?? Guid.NewGuid();
        var product = new Product(id, addProduct.Title, addProduct.Description, addProduct.Price);
        var stock = new Stock(product.Id, addProduct.Count);
        
        var transactItems = new List<TransactWriteItem>
        {
            new()
            {
                Put = new Put
                {
                    TableName = dbProvider.ProductsTable(),
                    Item = mapper.MapProduct(product),
                },
            },
            new()
            {
                Put = new Put
                {
                    TableName = dbProvider.StocksTable(),
                    Item = mapper.MapStock(stock),
                }
            }
        };

        await dbProvider.Client().TransactWriteItemsAsync(new TransactWriteItemsRequest
        {
            TransactItems = transactItems,
        });
    }
}
