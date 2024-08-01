using CartService.Dto;
using CartService.Services;
using Microsoft.AspNetCore.Mvc;

namespace CartService.Controllers;

[Route("api/order")]
public class OrderController(ICartService cartService,
    IOrderService orderService,
    IUserService userService,
    IHttpContextAccessor httpContextAccessor) : BaseController(httpContextAccessor, userService)
{
    [HttpPut]
    public async Task<IActionResult> Checkout([FromBody] AddOrderDto addOrderDto)
    {
        var userId = await GetUserId();
        
        if (userId is null)
            return Unauthorized();
        
        var cart = await cartService.FindByUserId(userId);
        
        if (cart is null || cart.Items.Count == 0)
            return BadRequest("Cart is empty");

        await orderService.Checkout(addOrderDto, cart);

        return Ok();
    }
    
    [HttpGet]
    public async Task<IActionResult> FindUserOrders()
    {
        var userId = await GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await orderService.FindByUserId(userId);

        return Ok(result);
    }
}