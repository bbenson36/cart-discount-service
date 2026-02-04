namespace CartDiscountService.Validation;

using CartDiscountService.Models;

public static class CartRequestValidator
{
    public static (bool IsValid, string? Error) Validate(CartRequest? request)
    {
        if (request is null)
            return (false, "Request body is required.");

        if (request.DiscountCodes is null || request.DiscountCodes.Count == 0)
            return (false, "At least one discount code is required.");

        foreach (var code in request.DiscountCodes)
        {
            if (string.IsNullOrWhiteSpace(code))
                return (false, "Discount codes must not be null or empty.");

            var (_, error) = DiscountCodeParser.TryParse(code);
            if (error is not null)
                return (false, error);
        }

        if (request.Items is not null)
        {
            foreach (var item in request.Items)
            {
                var itemError = ValidateLineItem(item);
                if (itemError is not null)
                    return (false, itemError);
            }

            var dupeError = ValidateNoDuplicatePriceMismatch(request.Items);
            if (dupeError is not null)
                return (false, dupeError);
        }

        return (true, null);
    }

    private static string? ValidateLineItem(LineItem item)
    {
        if (item.Name is null || string.IsNullOrWhiteSpace(item.Name))
            return "Line item name is required.";
        if (item.Price is null)
            return $"Price is required for item '{item.Name}'.";
        if (item.Price < 0)
            return $"Price must not be negative for item '{item.Name}'.";
        if (item.Quantity is null)
            return $"Quantity is required for item '{item.Name}'.";
        if (item.Quantity <= 0)
            return $"Quantity must be greater than 0 for item '{item.Name}'.";
        return null;
    }

    private static string? ValidateNoDuplicatePriceMismatch(List<LineItem> items)
    {
        var seen = new Dictionary<string, int>();
        foreach (var item in items)
        {
            if (item.Name is null) continue;

            if (seen.TryGetValue(item.Name, out var existingPrice))
            {
                if (item.Price != existingPrice)
                    return $"Duplicate item '{item.Name}' has conflicting prices.";
            }
            else
            {
                seen[item.Name] = item.Price!.Value;
            }
        }
        return null;
    }
}
