using Amazon;
using Amazon.DynamoDBv2;

namespace ProductService.Services;

public interface IDbProvider
{
    AmazonDynamoDBClient Client();
    string ProductsTable();
    string StocksTable();
}

public class DbProvider : IDbProvider
{
    public AmazonDynamoDBClient Client()
    {
        var clientConfig = new AmazonDynamoDBConfig
        {
            RegionEndpoint = RegionEndpoint.EUCentral1
        };
        return new AmazonDynamoDBClient(clientConfig);
    }

    public string ProductsTable()
    {
        return Environment.GetEnvironmentVariable("TABLE_PRODUCTS") ?? throw new ArgumentNullException($"TABLE_PRODUCTS");
    }

    public string StocksTable()
    {
        return Environment.GetEnvironmentVariable("TABLE_STOCKS") ?? throw new ArgumentNullException($"TABLE_STOCKS");
    }
}