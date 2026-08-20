using FluentValidation.TestHelper;
using StockFlow.Application.Products.CreateProduct;
using StockFlow.Domain.Products;

namespace StockFlow.Tests.Application.Products.CreateProduct;

public class CreateProductValidatorTests
{
    private readonly CreateProductValidator _validator = new();

    private static CreateProductCommand ValidCommand() => new(
        Sku: "SKU-100",
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
    public void Validate_WhenSkuIsEmpty_HasError()
    {
        var command = ValidCommand() with { Sku = string.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Sku);
    }

    [Fact]
    public void Validate_WhenPriceIsNegative_HasError()
    {
        var command = ValidCommand() with { Price = -1m };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Price);
    }

    [Fact]
    public void Validate_WhenMinimumStockThresholdIsNegative_HasError()
    {
        var command = ValidCommand() with { MinimumStockThreshold = -1 };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.MinimumStockThreshold);
    }

    [Fact]
    public void Validate_WhenCurrencyIsUndefined_HasError()
    {
        var command = ValidCommand() with { Currency = (Currency)999 };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Currency);
    }
}
