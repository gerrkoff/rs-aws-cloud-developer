using Amazon.DynamoDBv2.Model;
using ProductService.Models;

namespace ProductService.Services;

public interface IProductsService
{
    Task<List<ProductWithStock>> GetProducts();
    Task<Product?> GetProductById(Guid id);
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

    public async Task<Product?> GetProductById(Guid id)
    {
        var response = await dbProvider.Client().QueryAsync(new QueryRequest
        {
            TableName = dbProvider.ProductsTable(),
            KeyConditionExpression = "id = :id",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":id"] = new() { S = id.ToString() }
            }
        });
        
        return response.Items.Select(mapper.MapProduct).SingleOrDefault();
    }

    public async Task AddProduct(AddProductDto addProduct)
    {
        var product = new Product(Guid.NewGuid(), addProduct.Title, addProduct.Description, addProduct.Price);
        var stock = new Stock(product.Id, 0);
        
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
