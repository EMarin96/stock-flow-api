using FluentValidation.TestHelper;
using StockFlow.Application.Products.UpdateProduct;
using StockFlow.Domain.Products;

namespace StockFlow.Tests.Application.Products.UpdateProduct;

public class UpdateProductValidatorTests
{
    private readonly UpdateProductValidator _validator = new();

    private static UpdateProductCommand ValidCommand() => new(
        Id: Guid.NewGuid(),
        Name: "Widget",
        Description: "A widget",
        UnitOfMeasure: "unit",
        Price: 10m,
        Currency: Currency.USD,
        MinimumStockThreshold: 5);

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidCommand());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenPriceIsNegative_HasError()
    {
        var result = _validator.TestValidate(ValidCommand() with { Price = -1m });

        result.ShouldHaveValidationErrorFor(c => c.Price);
    }

    [Fact]
    public void Validate_WhenMinimumStockThresholdIsNegative_HasError()
    {
        var result = _validator.TestValidate(ValidCommand() with { MinimumStockThreshold = -1 });

        result.ShouldHaveValidationErrorFor(c => c.MinimumStockThreshold);
    }

    [Fact]
    public void Validate_WhenCurrencyIsUndefined_HasError()
    {
        var result = _validator.TestValidate(ValidCommand() with { Currency = (Currency)999 });

        result.ShouldHaveValidationErrorFor(c => c.Currency);
    }
}
