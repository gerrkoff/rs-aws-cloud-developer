namespace ImportService.Services;

public interface IEnvProvider
{
    string Bucket();
    string CatalogItemsQueue();
}

public class EnvProvider : IEnvProvider
{
    public string Bucket()
    {
        return Environment.GetEnvironmentVariable("IMPORT_BUCKET") ?? throw new ArgumentNullException($"IMPORT_BUCKET");
    }

    public string CatalogItemsQueue()
    {
        return Environment.GetEnvironmentVariable("CATALOG_ITEMS_QUEUE") ?? throw new ArgumentNullException($"CATALOG_ITEMS_QUEUE");
    }
}