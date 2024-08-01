namespace CartService.Types;

public record CartStatusType
{
    private readonly string _value;
    
    private CartStatusType(string value)
    {
        _value = value;
    }
    
    public static readonly CartStatusType Open = new("OPEN");
    public static readonly CartStatusType Ordered = new("ORDERED");
    
    public static CartStatusType FromString(string value)
    {
        return value switch
        {
            "OPEN" => Open,
            "ORDERED" => Ordered,
            _ => throw new ArgumentException($"Unknown value: {value}")
        };
    }
    
    public override string ToString()
    {
        return _value;
    }
}
