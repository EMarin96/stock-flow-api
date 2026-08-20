using FluentValidation.TestHelper;
using StockFlow.Application.Locations.GetLocations;

namespace StockFlow.Tests.Application.Locations.GetLocations;

public class GetLocationsValidatorTests
{
    private readonly GetLocationsValidator _validator = new();

    [Fact]
    public void Validate_WithValidPageAndPageSize_HasNoErrors()
    {
        var result = _validator.TestValidate(new GetLocationsQuery(1, 20));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenPageIsBelowOne_HasError(int page)
    {
        var result = _validator.TestValidate(new GetLocationsQuery(page, 20));

        result.ShouldHaveValidationErrorFor(q => q.Page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Validate_WhenPageSizeIsOutOfBounds_HasError(int pageSize)
    {
        var result = _validator.TestValidate(new GetLocationsQuery(1, pageSize));

        result.ShouldHaveValidationErrorFor(q => q.PageSize);
    }
}
