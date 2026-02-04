namespace CartDiscountService.Models;

public record CartResponse
{
    public required List<CartResponseItem> Items { get; init; }
    public required int Subtotal { get; init; }
    public required int Discount { get; init; }
    public required int Total { get; init; }
}

public record CartResponseItem
{
    public required string Name { get; init; }
    public required int Price { get; init; }
    public required int Quantity { get; init; }
}
