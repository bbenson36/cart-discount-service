namespace CartDiscountService.Tests;

using CartDiscountService.Models;
using CartDiscountService.Services;

public class DiscountCalculatorTests
{
    // === FLAT discount tests ===

    [Fact]
    public void Flat_SubtractsFixedAmountFromSubtotal()
    {
        var items = new List<LineItem>
        {
            new() { Name = "cake", Price = 1000, Quantity = 5 }
        };

        var result = DiscountCalculator.Calculate(items, ["FLAT_1000"]);

        Assert.Equal(5000, result.Subtotal);
        Assert.Equal(1000, result.Discount);
        Assert.Equal(4000, result.Total);
    }

    [Fact]
    public void Flat_DiscountExceedingSubtotal_FloorsAtZero()
    {
        var items = new List<LineItem>
        {
            new() { Name = "gum", Price = 100, Quantity = 1 }
        };

        var result = DiscountCalculator.Calculate(items, ["FLAT_5000"]);

        Assert.Equal(100, result.Subtotal);
        Assert.Equal(100, result.Discount);
        Assert.Equal(0, result.Total);
    }

    [Fact]
    public void Flat_CaseInsensitive()
    {
        var items = new List<LineItem>
        {
            new() { Name = "cake", Price = 1000, Quantity = 1 }
        };

        var result = DiscountCalculator.Calculate(items, ["flat_500"]);

        Assert.Equal(500, result.Discount);
        Assert.Equal(500, result.Total);
    }

    // === PERCENT discount tests ===

    [Fact]
    public void Percent_SubtractsPercentageFromSubtotal()
    {
        var items = new List<LineItem>
        {
            new() { Name = "cake", Price = 1000, Quantity = 10 }
        };

        var result = DiscountCalculator.Calculate(items, ["PERCENT_15"]);

        Assert.Equal(10000, result.Subtotal);
        Assert.Equal(1500, result.Discount);
        Assert.Equal(8500, result.Total);
    }

    [Fact]
    public void Percent_UsesBankersRounding_RoundsDown()
    {
        // 150 * 15 / 100 = 22.5 -> banker's rounds to 22 (round to even)
        var items = new List<LineItem>
        {
            new() { Name = "item", Price = 150, Quantity = 1 }
        };

        var result = DiscountCalculator.Calculate(items, ["PERCENT_15"]);

        Assert.Equal(150, result.Subtotal);
        Assert.Equal(22, result.Discount);
    }

    [Fact]
    public void Percent_UsesBankersRounding_RoundsUp()
    {
        // 250 * 15 / 100 = 37.5 -> banker's rounds to 38 (round to even)
        var items = new List<LineItem>
        {
            new() { Name = "item", Price = 250, Quantity = 1 }
        };

        var result = DiscountCalculator.Calculate(items, ["PERCENT_15"]);

        Assert.Equal(250, result.Subtotal);
        Assert.Equal(38, result.Discount);
    }

    [Fact]
    public void Percent_Zero_NoDiscount()
    {
        var items = new List<LineItem>
        {
            new() { Name = "cake", Price = 1000, Quantity = 5 }
        };

        var result = DiscountCalculator.Calculate(items, ["PERCENT_0"]);

        Assert.Equal(5000, result.Subtotal);
        Assert.Equal(0, result.Discount);
        Assert.Equal(5000, result.Total);
    }

    [Fact]
    public void Percent_100_FullDiscount()
    {
        var items = new List<LineItem>
        {
            new() { Name = "cake", Price = 1000, Quantity = 5 }
        };

        var result = DiscountCalculator.Calculate(items, ["PERCENT_100"]);

        Assert.Equal(5000, result.Subtotal);
        Assert.Equal(5000, result.Discount);
        Assert.Equal(0, result.Total);
    }

    // === BOGO discount tests ===

    [Fact]
    public void Bogo_EvenQuantity_HalfFree()
    {
        var items = new List<LineItem>
        {
            new() { Name = "cake", Price = 500, Quantity = 4 }
        };

        var result = DiscountCalculator.Calculate(items, ["BOGO"]);

        Assert.Equal(2000, result.Subtotal);
        Assert.Equal(1000, result.Discount);
        Assert.Equal(1000, result.Total);
    }

    [Fact]
    public void Bogo_OddQuantity_FloorsDown()
    {
        var items = new List<LineItem>
        {
            new() { Name = "cake", Price = 500, Quantity = 5 }
        };

        var result = DiscountCalculator.Calculate(items, ["BOGO"]);

        Assert.Equal(2500, result.Subtotal);
        Assert.Equal(1000, result.Discount);
        Assert.Equal(1500, result.Total);
    }

    [Fact]
    public void Bogo_QuantityOne_NoDiscount()
    {
        var items = new List<LineItem>
        {
            new() { Name = "cake", Price = 500, Quantity = 1 }
        };

        var result = DiscountCalculator.Calculate(items, ["BOGO"]);

        Assert.Equal(500, result.Subtotal);
        Assert.Equal(0, result.Discount);
        Assert.Equal(500, result.Total);
    }

    [Fact]
    public void Bogo_MultipleItemsDifferentPrices_OnlyCheapestDiscounted()
    {
        var items = new List<LineItem>
        {
            new() { Name = "cake", Price = 1000, Quantity = 2 },
            new() { Name = "cookie", Price = 200, Quantity = 4 }
        };

        var result = DiscountCalculator.Calculate(items, ["BOGO"]);

        // cheapest = 200, only cookie qualifies: floor(4/2) = 2 free at 200 = 400
        Assert.Equal(2800, result.Subtotal);
        Assert.Equal(400, result.Discount);
        Assert.Equal(2400, result.Total);
    }

    [Fact]
    public void Bogo_MultipleItemsSameCheapestPrice_AllDiscounted()
    {
        var items = new List<LineItem>
        {
            new() { Name = "cookie", Price = 200, Quantity = 4 },
            new() { Name = "muffin", Price = 200, Quantity = 3 }
        };

        var result = DiscountCalculator.Calculate(items, ["BOGO"]);

        // cookie: floor(4/2)=2 free, muffin: floor(3/2)=1 free = (2+1)*200 = 600
        Assert.Equal(1400, result.Subtotal);
        Assert.Equal(600, result.Discount);
        Assert.Equal(800, result.Total);
    }

    // === Multiple discount tests ===

    [Fact]
    public void MultipleDiscounts_CalculatedIndependentlyAndSummed()
    {
        var items = new List<LineItem>
        {
            new() { Name = "cake", Price = 1000, Quantity = 10 }
        };

        var result = DiscountCalculator.Calculate(items, ["FLAT_1000", "PERCENT_10"]);

        // FLAT_1000 = 1000, PERCENT_10 = 10000*10/100 = 1000, total = 2000
        Assert.Equal(10000, result.Subtotal);
        Assert.Equal(2000, result.Discount);
        Assert.Equal(8000, result.Total);
    }

    [Fact]
    public void MultipleDiscounts_ExceedingSubtotal_CappedAtSubtotal()
    {
        var items = new List<LineItem>
        {
            new() { Name = "cake", Price = 500, Quantity = 1 }
        };

        var result = DiscountCalculator.Calculate(items, ["FLAT_300", "FLAT_300"]);

        Assert.Equal(500, result.Subtotal);
        Assert.Equal(500, result.Discount);
        Assert.Equal(0, result.Total);
    }

    // === Empty cart ===

    [Fact]
    public void EmptyCart_ReturnsZeros()
    {
        var items = new List<LineItem>();

        var result = DiscountCalculator.Calculate(items, ["FLAT_1000"]);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.Subtotal);
        Assert.Equal(0, result.Discount);
        Assert.Equal(0, result.Total);
    }

    // === Duplicate line items ===

    [Fact]
    public void DuplicateItems_CombinedByName()
    {
        var items = new List<LineItem>
        {
            new() { Name = "cake", Price = 1000, Quantity = 2 },
            new() { Name = "cake", Price = 1000, Quantity = 3 }
        };

        var result = DiscountCalculator.Calculate(items, ["FLAT_0"]);

        Assert.Single(result.Items);
        Assert.Equal("cake", result.Items[0].Name);
        Assert.Equal(1000, result.Items[0].Price);
        Assert.Equal(5, result.Items[0].Quantity);
        Assert.Equal(5000, result.Subtotal);
    }

    // === Price of zero ===

    [Fact]
    public void ItemWithZeroPrice_HandledCorrectly()
    {
        var items = new List<LineItem>
        {
            new() { Name = "freebie", Price = 0, Quantity = 3 },
            new() { Name = "cake", Price = 1000, Quantity = 2 }
        };

        var result = DiscountCalculator.Calculate(items, ["BOGO"]);

        // cheapest = 0, BOGO on 0-price item: floor(3/2)*0 = 0 discount
        Assert.Equal(2000, result.Subtotal);
        Assert.Equal(0, result.Discount);
        Assert.Equal(2000, result.Total);
    }
}
