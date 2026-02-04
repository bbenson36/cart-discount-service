namespace CartDiscountService.Tests;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

public class EndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public EndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostDiscount_ValidRequest_ReturnsOkWithCorrectTotals()
    {
        var request = new
        {
            items = new[] { new { name = "cake", price = 1000, quantity = 5 } },
            discountCodes = new[] { "FLAT_1000" }
        };

        var response = await _client.PostAsJsonAsync("/cart/discount", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(5000, body.GetProperty("subtotal").GetInt32());
        Assert.Equal(1000, body.GetProperty("discount").GetInt32());
        Assert.Equal(4000, body.GetProperty("total").GetInt32());
    }

    [Fact]
    public async Task PostDiscount_EmptyDiscountCodes_Returns400()
    {
        var request = new
        {
            items = new[] { new { name = "cake", price = 1000, quantity = 5 } },
            discountCodes = Array.Empty<string>()
        };

        var response = await _client.PostAsJsonAsync("/cart/discount", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostDiscount_InvalidDiscountCode_Returns400()
    {
        var request = new
        {
            items = new[] { new { name = "cake", price = 1000, quantity = 5 } },
            discountCodes = new[] { "INVALID_CODE" }
        };

        var response = await _client.PostAsJsonAsync("/cart/discount", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostDiscount_PercentOutOfRange_Returns400()
    {
        var request = new
        {
            items = new[] { new { name = "cake", price = 1000, quantity = 5 } },
            discountCodes = new[] { "PERCENT_150" }
        };

        var response = await _client.PostAsJsonAsync("/cart/discount", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostDiscount_NegativePrice_Returns400()
    {
        var request = new
        {
            items = new[] { new { name = "cake", price = -100, quantity = 5 } },
            discountCodes = new[] { "FLAT_100" }
        };

        var response = await _client.PostAsJsonAsync("/cart/discount", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostDiscount_ZeroQuantity_Returns400()
    {
        var request = new
        {
            items = new[] { new { name = "cake", price = 1000, quantity = 0 } },
            discountCodes = new[] { "FLAT_100" }
        };

        var response = await _client.PostAsJsonAsync("/cart/discount", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostDiscount_EmptyCart_ReturnsOkWithZeros()
    {
        var request = new
        {
            items = Array.Empty<object>(),
            discountCodes = new[] { "FLAT_1000" }
        };

        var response = await _client.PostAsJsonAsync("/cart/discount", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(0, body.GetProperty("subtotal").GetInt32());
        Assert.Equal(0, body.GetProperty("discount").GetInt32());
        Assert.Equal(0, body.GetProperty("total").GetInt32());
    }

    [Fact]
    public async Task PostDiscount_DuplicateItemsDifferentPrices_Returns400()
    {
        var request = new
        {
            items = new[]
            {
                new { name = "cake", price = 1000, quantity = 2 },
                new { name = "cake", price = 500, quantity = 3 }
            },
            discountCodes = new[] { "FLAT_100" }
        };

        var response = await _client.PostAsJsonAsync("/cart/discount", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostDiscount_ReturnsCorrectJsonShape()
    {
        var request = new
        {
            items = new[] { new { name = "cake", price = 1000, quantity = 2 } },
            discountCodes = new[] { "PERCENT_50" }
        };

        var response = await _client.PostAsJsonAsync("/cart/discount", request);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        // Verify camelCase keys exist
        Assert.True(body.TryGetProperty("items", out var itemsEl));
        Assert.True(body.TryGetProperty("subtotal", out _));
        Assert.True(body.TryGetProperty("discount", out _));
        Assert.True(body.TryGetProperty("total", out _));

        // Verify item shape
        var firstItem = itemsEl.EnumerateArray().First();
        Assert.True(firstItem.TryGetProperty("name", out _));
        Assert.True(firstItem.TryGetProperty("price", out _));
        Assert.True(firstItem.TryGetProperty("quantity", out _));
    }
}
