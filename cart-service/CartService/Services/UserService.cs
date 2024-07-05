using System.Text;
using CartService.Data;
using Microsoft.Extensions.Caching.Memory;

namespace CartService.Services;

public interface IUserService
{
    Task<string?> AuthenticateAndGetUserId(string authToken);
}

public class UserService(IRepository repository, IMemoryCache memoryCache) : IUserService
{
    public async Task<string?> AuthenticateAndGetUserId(string authToken)
    {
        return await memoryCache.GetOrCreateAsync<string?>("auth_" + authToken, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

            if (!DecodeBasicAuthToken(authToken, out var userId, out var password))
                return null;

            var userExists = await repository.CheckUserExists(userId, password);

            return userExists ? userId : null;
        });
    }
    
    private bool DecodeBasicAuthToken(string authToken, out string userId, out string password)
    {
        userId = string.Empty;
        password = string.Empty;
        
        try
        {
            var tokenParts = authToken.Split(' ');
            if (tokenParts.Length != 2 || tokenParts[0] != "Basic")
                return false;
            
            var token = tokenParts[1];
            var credentialString = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var credentials = credentialString.Split(':');
            
            if (credentials.Length != 2)
                return false;
            
            userId = credentials[0];
            password = credentials[1];

            return true;
        }
        catch
        {
            return false;
        }
    }
}