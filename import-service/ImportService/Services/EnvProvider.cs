namespace ImportService.Services;

public interface IEnvProvider
{
    string Bucket();
}

public class EnvProvider : IEnvProvider
{
    public string Bucket()
    {
        return Environment.GetEnvironmentVariable("IMPORT_BUCKET") ?? throw new ArgumentNullException($"IMPORT_BUCKET");
    }
}