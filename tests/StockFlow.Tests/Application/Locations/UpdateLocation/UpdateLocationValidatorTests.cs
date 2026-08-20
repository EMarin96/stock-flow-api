using FluentValidation.TestHelper;
using StockFlow.Application.Locations.UpdateLocation;
using StockFlow.Domain.Locations;

namespace StockFlow.Tests.Application.Locations.UpdateLocation;

public class UpdateLocationValidatorTests
{
    private readonly UpdateLocationValidator _validator = new();

    private static UpdateLocationCommand ValidCommand() => new(
        Id: Guid.NewGuid(),
        Name: "Main Warehouse",
        AddressLine1: "123 Main St",
        AddressLine2: null,
        AddressLine3: null,
        State: "CA",
        City: "Los Angeles",
        Country: Country.US);

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidCommand());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenIdIsEmpty_HasError()
    {
        var command = ValidCommand() with { Id = Guid.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Id);
    }

    [Fact]
    public void Validate_WhenNameIsEmpty_HasError()
    {
        var command = ValidCommand() with { Name = string.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void Validate_WhenStateIsEmpty_HasError()
    {
        var command = ValidCommand() with { State = string.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.State);
    }

    [Fact]
    public void Validate_WhenCityIsEmpty_HasError()
    {
        var command = ValidCommand() with { City = string.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.City);
    }

    [Fact]
    public void Validate_WhenCountryIsNotAValidEnumValue_HasError()
    {
        var command = ValidCommand() with { Country = (Country)999 };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Country);
    }
}
