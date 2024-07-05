using CartService.Data;
using CartService.Dto;
using CartService.Entities;
using CartService.Types;

namespace CartService.Services;

public interface ICartService
{
    Task<CartEntity?> FindByUserId(string userId);
    Task<CartEntity> FindOrCreateByUserId(string userId);
    Task UpdateByUserId(string userId, CartItemDto cartItem);
}

public class CartService(IRepository repository) : ICartService
{
    public async Task<CartEntity?> FindByUserId(string userId)
    {
        return await repository.GetCartByUserId(userId);
    }
    
    public async Task<CartEntity> FindOrCreateByUserId(string userId)
    {
        var existingCart = await FindByUserId(userId);
        
        if (existingCart is not null)
        {
            return existingCart;
        }
        
        var cart = new CartEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Status = CartStatusType.Open,
            Items = [],
        };

        await repository.CreateCart(cart);
        
        return cart;
    }
    
    public async Task UpdateByUserId(string userId, CartItemDto cartItem)
    {
        var cart = await FindOrCreateByUserId(userId);
        
        var existingItem = cart.Items.FirstOrDefault(x => x.ProductId == cartItem.Product.Id);
        
        if (existingItem is not null)
        {
            await repository.UpdateCartItemCount(cart.Id, cartItem.Product.Id, cartItem.Count);
        }
        else
        {
            await repository.CreateCartItem(cart.Id, new CartItemEntity
            {
                CartId = cart.Id,
                ProductId = cartItem.Product.Id,
                Count = cartItem.Count,
                Price = cartItem.Product.Price,
            });
        }
    }
}
