using FluentValidation.TestHelper;
using StockFlow.Application.Products.GetProducts;

namespace StockFlow.Tests.Application.Products.GetProducts;

public class GetProductsValidatorTests
{
    private readonly GetProductsValidator _validator = new();

    [Fact]
    public void Validate_WithValidPageAndPageSize_HasNoErrors()
    {
        var result = _validator.TestValidate(new GetProductsQuery(1, 20));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenPageIsBelowOne_HasError(int page)
    {
        var result = _validator.TestValidate(new GetProductsQuery(page, 20));

        result.ShouldHaveValidationErrorFor(q => q.Page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Validate_WhenPageSizeIsOutOfBounds_HasError(int pageSize)
    {
        var result = _validator.TestValidate(new GetProductsQuery(1, pageSize));

        result.ShouldHaveValidationErrorFor(q => q.PageSize);
    }
}
