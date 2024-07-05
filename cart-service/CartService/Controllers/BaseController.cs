using CartService.Dto;
using CartService.Services;
using Microsoft.AspNetCore.Mvc;

namespace CartService.Controllers;

public class BaseController(IHttpContextAccessor httpContextAccessor, IUserService userService) : ControllerBase
{
    public async Task<string?> GetUserId()
    {
        var authTokens = httpContextAccessor.HttpContext?.Request.Headers["Authorization"];
        var authToken = authTokens.GetValueOrDefault().ToString();

        var userId = string.IsNullOrEmpty(authToken)
            ? null
            : await userService.AuthenticateAndGetUserId(authToken);

        return userId;
    }
}
