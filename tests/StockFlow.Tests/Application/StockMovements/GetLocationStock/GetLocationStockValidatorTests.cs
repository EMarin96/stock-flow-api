using FluentValidation.TestHelper;
using StockFlow.Application.StockMovements.GetLocationStock;

namespace StockFlow.Tests.Application.StockMovements.GetLocationStock;

public class GetLocationStockValidatorTests
{
    private readonly GetLocationStockValidator _validator = new();

    [Fact]
    public void Validate_WithValidQuery_HasNoErrors()
    {
        var result = _validator.TestValidate(new GetLocationStockQuery(Guid.NewGuid(), null, 1, 20));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenPageIsBelowOne_HasError(int page)
    {
        var result = _validator.TestValidate(new GetLocationStockQuery(Guid.NewGuid(), null, page, 20));

        result.ShouldHaveValidationErrorFor(q => q.Page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Validate_WhenPageSizeIsOutOfBounds_HasError(int pageSize)
    {
        var result = _validator.TestValidate(new GetLocationStockQuery(Guid.NewGuid(), null, 1, pageSize));

        result.ShouldHaveValidationErrorFor(q => q.PageSize);
    }
}
