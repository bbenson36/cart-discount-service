namespace CartDiscountService.Models;

public enum DiscountType { Flat, Percent, Bogo }

public record ParsedDiscount(DiscountType Type, int Value = 0);

public static class DiscountCodeParser
{
    public static (ParsedDiscount? Discount, string? Error) TryParse(string code)
    {
        var upper = code.ToUpperInvariant();

        if (upper == "BOGO")
            return (new ParsedDiscount(DiscountType.Bogo), null);

        if (upper.StartsWith("FLAT_"))
        {
            var valueStr = upper["FLAT_".Length..];
            if (!int.TryParse(valueStr, out var value) || value < 0)
                return (null, $"Invalid discount code: '{code}'. FLAT_X requires X to be a non-negative integer.");
            return (new ParsedDiscount(DiscountType.Flat, value), null);
        }

        if (upper.StartsWith("PERCENT_"))
        {
            var valueStr = upper["PERCENT_".Length..];
            if (!int.TryParse(valueStr, out var value))
                return (null, $"Invalid discount code: '{code}'. PERCENT_X requires X to be an integer.");
            if (value < 0 || value > 100)
                return (null, $"Invalid discount code: '{code}'. PERCENT_X requires X to be between 0 and 100.");
            return (new ParsedDiscount(DiscountType.Percent, value), null);
        }

        return (null, $"Unrecognized discount code: '{code}'.");
    }
}
