using CartDiscountService.Models;
using CartDiscountService.Services;
using CartDiscountService.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/cart/discount", (CartRequest? request) =>
{
    var (isValid, error) = CartRequestValidator.Validate(request);
    if (!isValid)
    {
        return Results.BadRequest(new { error });
    }

    var result = DiscountCalculator.Calculate(
        request!.Items ?? [],
        request.DiscountCodes!);

    return Results.Ok(result);
})
.WithName("ApplyCartDiscount");

app.Run();

public partial class Program { }
