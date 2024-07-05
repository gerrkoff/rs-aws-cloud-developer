using System.Text.Json;
using CartService.Entities;
using CartService.Types;
using Dapper;
using Npgsql;

namespace CartService.Data;

public interface IRepository
{
    Task CreateCart(CartEntity cartEntity);
    Task<CartEntity?> GetCartByUserId(string userId);
    Task<IEnumerable<CartEntity>> GetCartsByUserId(string userId);
    Task UpdateCartItemCount(Guid cartId, Guid productId, int count);
    Task CreateCartItem(Guid cartId, CartItemEntity cartItem);
    Task Checkout(OrderEntity order);
    Task<IEnumerable<OrderEntity>> GetOrdersByUserId(string userId);
    Task<bool> CheckUserExists(string userId, string password);
}

public class Repository : IRepository
{
    private readonly string _connectionString;

    public Repository()
    {
        _connectionString = Environment.GetEnvironmentVariable("RDS_CONNECTION_STRING") ?? throw new ArgumentNullException(nameof(_connectionString));
    }

    public async Task CreateCart(CartEntity cartEntity)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        var sql = "INSERT INTO carts (id, user_id, created_at, updated_at, status) VALUES (@Id, @UserId, @CreatedAt, @UpdatedAt, @Status)";
        await connection.ExecuteAsync(sql, new
        {
            cartEntity.Id,
            cartEntity.UserId,
            cartEntity.CreatedAt,
            cartEntity.UpdatedAt,
            Status = cartEntity.Status.ToString(),
        });
    }
    
    public async Task<CartEntity?> GetCartByUserId(string userId)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        var cartSql = "SELECT id, user_id as userId, created_at as createdAt, updated_at as updatedAt, status FROM carts WHERE user_id = @UserId AND status = 'OPEN' LIMIT 1";
        var dbEntity = await connection.QueryFirstOrDefaultAsync<CartEntity.DbEntity>(cartSql, new { UserId = userId });
        
        if (dbEntity is null)
        {
            return null;
        }
        
        var cartItemsSql = "SELECT cart_id as cartId, product_id as productId, count, price FROM cart_items WHERE cart_id = @CartId";
        var cartItems = await connection.QueryAsync<CartItemEntity>(cartItemsSql, new { CartId = dbEntity.Id });
        
        return dbEntity.ToEntity(cartItems.ToList());
    }
    
    public async Task<IEnumerable<CartEntity>> GetCartsByUserId(string userId)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        var cartSql = "SELECT id, user_id as userId, created_at as createdAt, updated_at as updatedAt, status FROM carts WHERE user_id = @UserId";
        var dbEntities = (await connection.QueryAsync<CartEntity.DbEntity>(cartSql, new { UserId = userId })).ToList();
        
        var cartItemsSql = "SELECT cart_id as cartId, product_id as productId, count, price FROM cart_items WHERE cart_id = ANY(@Ids)";
        var cartItems = await connection.QueryAsync<CartItemEntity>(cartItemsSql, new { Ids = dbEntities.Select(x => x.Id).ToArray() });
        
        return dbEntities.Select(x => x.ToEntity(cartItems.Where(y => y.CartId == x.Id).ToList()));
    }
    
    public async Task UpdateCartItemCount(Guid cartId, Guid productId, int count)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        var sql = "UPDATE cart_items SET count = @Count WHERE cart_id = @CartId AND product_id = @ProductId";
        await connection.ExecuteAsync(sql, new { CartId = cartId, ProductId = productId, Count = count });
    }
    
    public async Task CreateCartItem(Guid cartId, CartItemEntity cartItem)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        
        var sql = "INSERT INTO cart_items (cart_id, product_id, count, price) VALUES (@CartId, @ProductId, @Count, @Price)";
        await connection.ExecuteAsync(sql, new { CartId = cartId, cartItem.ProductId, cartItem.Count, cartItem.Price });
    }

    public async Task Checkout(OrderEntity order)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        
        var orderSql = "INSERT INTO orders (id, cart_id, user_id, payment, delivery, comments, status, total) VALUES (@Id, @CartId, @UserId, @Payment::json, @Delivery::json, @Comments, @Status, @Total)";
        await connection.ExecuteAsync(orderSql, new
        {
            order.Id,
            order.UserId,
            order.CartId,
            Payment = JsonSerializer.Serialize(order.Payment),
            Delivery = JsonSerializer.Serialize(order.Delivery),
            order.Comments,
            Status = order.Status.ToString(),
            order.Total,
        }, transaction: transaction);
        
        var cartSql = "UPDATE carts SET status = @Status WHERE id = @CartId";
        await connection.ExecuteAsync(cartSql, new { order.CartId, Status = CartStatusType.Ordered.ToString() }, transaction: transaction);

        await transaction.CommitAsync();
    }

    public async Task<IEnumerable<OrderEntity>> GetOrdersByUserId(string userId)
    {   
        await using var connection = new NpgsqlConnection(_connectionString);

        var sql = "SELECT id, cart_id as cartId, user_id as userId, payment, delivery, comments, status, total FROM orders WHERE user_id = @UserId";
        var dbEntities = await connection.QueryAsync<OrderEntity.DbEntity>(sql, new { UserId = userId });
        
        return dbEntities.Select(x => x.ToEntity());
    }

    public async Task<bool> CheckUserExists(string userId, string password)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        var sql = "SELECT COUNT(*) FROM users WHERE id = @UserId AND password = @Password";
        return await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId, Password = password }) > 0;
    }
}
