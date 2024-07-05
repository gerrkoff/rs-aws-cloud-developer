using CartService.Data;
using CartService.Dto;
using CartService.Entities;
using CartService.Types;

namespace CartService.Services;

public interface IOrderService
{
    Task Checkout(AddOrderDto addOrderDto, CartEntity cart);
    Task<IEnumerable<OrderDto>> FindByUserId(string userId);
}

public class OrderService(IRepository repository) : IOrderService
{
    public async Task<IEnumerable<OrderDto>> FindByUserId(string userId)
    {
        var order = await repository.GetOrdersByUserId(userId);
        
        var carts = (await repository.GetCartsByUserId(userId)).ToList();

        return order.Select(x => new OrderDto
        {
            Id = x.Id,
            CartId = x.CartId,
            UserId = x.UserId,
            Status = x.Status.ToString(),
            Comments = x.Comments,
            Total = x.Total,
            Address = new OrderDto.OrderDtoAddress
            {
                Address = x.Delivery.Address,
                FirstName = x.Delivery.FirstName,
                LastName = x.Delivery.LastName,
            },
            StatusHistory = [new OrderDto.OrderDtoStatus { Status = x.Status.ToString() }],
            Items = carts.Single(y => y.Id == x.CartId).Items.Select(y => new CartItemDto
            {
                Count = y.Count,
                Product = new CartItemDto.CartItemDtoProduct()
                {
                    Id = y.ProductId,
                    Price = y.Price,
                }
            }).ToList(),
        });
    }
    
    public async Task Checkout(AddOrderDto addOrderDto, CartEntity cart)
    {
        var order = new OrderEntity
        {
            Id = Guid.NewGuid(),
            UserId = cart.UserId,
            CartId = cart.Id,
            Payment = new OrderEntity.OrderEntityPayment
            {
                Address = addOrderDto.Address.Address,
            },
            Delivery = new OrderEntity.OrderEntityDelivery
            {
                Address = addOrderDto.Address.Address,
                FirstName = addOrderDto.Address.FirstName,
                LastName = addOrderDto.Address.LastName,
            },
            Comments = addOrderDto.Address.Comment,
            Status = OrderStatusType.Open,
            Total = cart.Items.Sum(x => x.Count * x.Price),
        };

        await repository.Checkout(order);
    }
}