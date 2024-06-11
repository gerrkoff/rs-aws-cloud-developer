namespace ProductService.Models;

public record Product(
    Guid Id,
    string Title,
    string Description,
    int Price);
