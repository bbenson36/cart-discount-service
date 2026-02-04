namespace CartDiscountService.Services;

using CartDiscountService.Models;

public static class DiscountCalculator
{
    public static CartResponse Calculate(List<LineItem> items, List<string> discountCodes)
    {
        var consolidated = DeduplicateItems(items);

        var subtotal = consolidated.Sum(i => i.Price * i.Quantity);

        if (consolidated.Count == 0)
        {
            return new CartResponse
            {
                Items = [],
                Subtotal = 0,
                Discount = 0,
                Total = 0
            };
        }

        var totalDiscount = 0m;
        foreach (var code in discountCodes)
        {
            totalDiscount += CalculateSingleDiscount(code, consolidated, subtotal);
        }

        var discountCents = (int)Math.Round(totalDiscount, MidpointRounding.ToEven);
        var actualDiscount = Math.Min(discountCents, subtotal);
        var total = Math.Max(subtotal - actualDiscount, 0);

        return new CartResponse
        {
            Items = consolidated.Select(i => new CartResponseItem
            {
                Name = i.Name,
                Price = i.Price,
                Quantity = i.Quantity
            }).ToList(),
            Subtotal = subtotal,
            Discount = actualDiscount,
            Total = total
        };
    }

    private static List<(string Name, int Price, int Quantity)> DeduplicateItems(List<LineItem> items)
    {
        return items
            .GroupBy(i => i.Name!)
            .Select(g => (
                Name: g.Key,
                Price: g.First().Price!.Value,
                Quantity: g.Sum(i => i.Quantity!.Value)
            ))
            .ToList();
    }

    private static decimal CalculateSingleDiscount(
        string code,
        List<(string Name, int Price, int Quantity)> items,
        int subtotal)
    {
        var upper = code.ToUpperInvariant();

        if (upper == "BOGO")
            return CalculateBogo(items);

        if (upper.StartsWith("FLAT_"))
        {
            var amount = int.Parse(upper["FLAT_".Length..]);
            return amount;
        }

        if (upper.StartsWith("PERCENT_"))
        {
            var percent = int.Parse(upper["PERCENT_".Length..]);
            return subtotal * percent / 100m;
        }

        throw new InvalidOperationException($"Unrecognized discount code: {code}");
    }

    private static decimal CalculateBogo(List<(string Name, int Price, int Quantity)> items)
    {
        if (items.Count == 0) return 0;

        var cheapestPrice = items.Min(i => i.Price);

        var discount = 0m;
        foreach (var item in items.Where(i => i.Price == cheapestPrice))
        {
            var freeCount = item.Quantity / 2;
            discount += freeCount * cheapestPrice;
        }

        return discount;
    }
}
