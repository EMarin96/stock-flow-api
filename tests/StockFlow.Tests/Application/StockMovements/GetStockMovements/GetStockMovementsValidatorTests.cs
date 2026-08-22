using FluentValidation.TestHelper;
using StockFlow.Application.StockMovements.GetStockMovements;
using StockFlow.Domain.StockMovements;

namespace StockFlow.Tests.Application.StockMovements.GetStockMovements;

public class GetStockMovementsValidatorTests
{
    private readonly GetStockMovementsValidator _validator = new();

    [Fact]
    public void Validate_WithValidQuery_HasNoErrors()
    {
        var result = _validator.TestValidate(new GetStockMovementsQuery(1, 20, null, null, null));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenPageIsBelowOne_HasError(int page)
    {
        var result = _validator.TestValidate(new GetStockMovementsQuery(page, 20, null, null, null));

        result.ShouldHaveValidationErrorFor(q => q.Page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Validate_WhenPageSizeIsOutOfBounds_HasError(int pageSize)
    {
        var result = _validator.TestValidate(new GetStockMovementsQuery(1, pageSize, null, null, null));

        result.ShouldHaveValidationErrorFor(q => q.PageSize);
    }

    [Fact]
    public void Validate_WhenTypeIsNotAValidEnumValue_HasError()
    {
        var result = _validator.TestValidate(new GetStockMovementsQuery(1, 20, null, null, (MovementType)999));

        result.ShouldHaveValidationErrorFor(q => q.Type);
    }
}
