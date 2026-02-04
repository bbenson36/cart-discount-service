namespace CartDiscountService.Models;

public record CartRequest
{
    public List<LineItem>? Items { get; init; }
    public List<string>? DiscountCodes { get; init; }
}

public record LineItem
{
    public string? Name { get; init; }
    public int? Price { get; init; }
    public int? Quantity { get; init; }
}
