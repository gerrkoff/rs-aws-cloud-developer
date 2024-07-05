namespace CartService.Types;

public record OrderStatusType
{
    private readonly string _value;
    
    private OrderStatusType(string value)
    {
        _value = value;
    }
    
    public static readonly OrderStatusType Open = new("OPEN");
    
    public static OrderStatusType FromString(string value)
    {
        return value switch
        {
            "OPEN" => Open,
            _ => throw new ArgumentException($"Unknown value: {value}")
        };
    }
    
    public override string ToString()
    {
        return _value;
    }
}
