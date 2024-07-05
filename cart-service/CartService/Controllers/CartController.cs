using CartService.Dto;
using CartService.Services;
using Microsoft.AspNetCore.Mvc;

namespace CartService.Controllers;

[Route("api/profile/cart")]
public class CartController(ICartService cartService,
    IUserService userService,
    IHttpContextAccessor httpContextAccessor) : BaseController(httpContextAccessor, userService)
{    
    [HttpGet]
    public async Task<IActionResult> FindUserCart()
    {
        var userId = await GetUserId();
        if (userId is null)
            return Unauthorized();
        
        var cart = await cartService.FindOrCreateByUserId(userId);

        var result = cart.Items.Select(x => new CartItemDto
        {
            Product = new CartItemDto.CartItemDtoProduct
            {
                Id = x.ProductId,
                Price = x.Price,
            },
            Count = x.Count,
        });
        
        return Ok(result);
    }
    
    [HttpPut]
    public async Task<IActionResult> UpdateUserCart([FromBody] CartItemDto cartItem)
    {
        var userId = await GetUserId();
        if (userId is null)
            return Unauthorized();
        
        await cartService.UpdateByUserId(userId, cartItem);
        
        return Ok();
    }
}
