namespace ProductService.Services;

public interface IEnvProvider
{
    string ProductsTable();
    string StocksTable();
    string CatalogItemCreatedTopic();
}

public class EnvProvider : IEnvProvider
{
    public string ProductsTable()
    {
        return Environment.GetEnvironmentVariable("TABLE_PRODUCTS") ?? throw new ArgumentNullException($"TABLE_PRODUCTS");
    }

    public string StocksTable()
    {
        return Environment.GetEnvironmentVariable("TABLE_STOCKS") ?? throw new ArgumentNullException($"TABLE_STOCKS");
    }

    public string CatalogItemCreatedTopic()
    {
        return Environment.GetEnvironmentVariable("CATALOG_ITEM_CREATED_TOPIC") ?? throw new ArgumentNullException($"CATALOG_ITEM_CREATED_TOPIC");
    }
}