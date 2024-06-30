using System.Diagnostics.CodeAnalysis;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Common.Models;

namespace ProductService.Services;

public interface IProductsService
{
    Task<List<ProductWithStock>> GetProducts();
    Task<ProductWithStock?> GetProductById(Guid id);
    Task<ProductWithStock> AddProduct(AddProductDto addProduct);
    bool IsProductValid([NotNullWhen(true)] AddProductDto? addProduct);
}

public class ProductsService(
    IEnvProvider envProvider,
    IMapper mapper) : IProductsService
{
    public async Task<List<ProductWithStock>> GetProducts()
    {
        var productsWithStock = new List<ProductWithStock>();
        
        var productsResponse = await Client().ScanAsync(new ScanRequest
        {
            TableName = envProvider.ProductsTable(),
        });

        var products = productsResponse.Items
            .Select(mapper.MapProduct)
            .ToDictionary(x => x.Id, x => x);

        foreach (var keysChunk in products.Keys.Chunk(100))
        {
            var stocksResponse = await Client().BatchGetItemAsync(new BatchGetItemRequest
            {
                RequestItems = new Dictionary<string, KeysAndAttributes>
                {
                    [envProvider.StocksTable()] = new()
                    {
                        Keys = keysChunk.Select(id => new Dictionary<string, AttributeValue>
                        {
                            ["productId"] = new() { S = id.ToString() }
                        }).ToList()
                    },
                },
            });
            
            foreach (var item in stocksResponse.Responses[envProvider.StocksTable()])
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
        var productItemTask = Client().QueryAsync(new QueryRequest
        {
            TableName = envProvider.ProductsTable(),
            KeyConditionExpression = "id = :id",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":id"] = new() { S = id.ToString() }
            }
        });
        
        var stockItemTask = Client().QueryAsync(new QueryRequest
        {
            TableName = envProvider.StocksTable(),
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

    public async Task<ProductWithStock> AddProduct(AddProductDto addProduct)
    {
        var id = addProduct.Id ?? Guid.NewGuid();
        var product = new Product
        {
            Id = id,
            Title = addProduct.Title,
            Description = addProduct.Description,
            Price = addProduct.Price
        };
        var stock = new Stock
        {
            ProductId = product.Id,
            Count = addProduct.Count,
        };
        
        var transactItems = new List<TransactWriteItem>
        {
            new()
            {
                Put = new Put
                {
                    TableName = envProvider.ProductsTable(),
                    Item = mapper.MapProduct(product),
                },
            },
            new()
            {
                Put = new Put
                {
                    TableName = envProvider.StocksTable(),
                    Item = mapper.MapStock(stock),
                }
            }
        };

        await Client().TransactWriteItemsAsync(new TransactWriteItemsRequest
        {
            TransactItems = transactItems,
        });

        return mapper.CreateProductWithStock(product, stock);
    }
    
    public bool IsProductValid(AddProductDto? addProduct)
    {
        return addProduct is { Price: > 0, Count: > 0 }
               && !string.IsNullOrEmpty(addProduct.Title)
               && !string.IsNullOrEmpty(addProduct.Description);
    }
    
    private AmazonDynamoDBClient Client()
    {
        var clientConfig = new AmazonDynamoDBConfig
        {
            RegionEndpoint = RegionEndpoint.EUCentral1
        };
        return new AmazonDynamoDBClient(clientConfig);
    }
}
