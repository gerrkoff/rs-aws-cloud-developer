namespace ProductService.Models;

public class Stock(Guid productId, int count)
{
    public Guid ProductId { get; private set; } = productId;

    public int Count { get; private set; } = count;
}
