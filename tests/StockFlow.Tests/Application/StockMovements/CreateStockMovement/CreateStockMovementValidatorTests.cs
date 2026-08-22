using FluentValidation.TestHelper;
using StockFlow.Application.StockMovements.CreateStockMovement;
using StockFlow.Domain.StockMovements;

namespace StockFlow.Tests.Application.StockMovements.CreateStockMovement;

public class CreateStockMovementValidatorTests
{
    private readonly CreateStockMovementValidator _validator = new();

    private static readonly Guid ProductId = Guid.NewGuid();
    private static readonly Guid SourceLocationId = Guid.NewGuid();
    private static readonly Guid DestinationLocationId = Guid.NewGuid();

    private static CreateStockMovementCommand ValidInCommand() => new(
        ProductId, MovementType.In, 10, SourceLocationId: null, DestinationLocationId, Direction: null);

    [Fact]
    public void Validate_WithValidInCommand_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidInCommand());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenQuantityIsZeroOrNegative_HasError(int quantity)
    {
        var result = _validator.TestValidate(ValidInCommand() with { Quantity = quantity });

        result.ShouldHaveValidationErrorFor(c => c.Quantity);
    }

    [Fact]
    public void Validate_WhenTypeIsNotAValidEnumValue_HasError()
    {
        var result = _validator.TestValidate(ValidInCommand() with { Type = (MovementType)999 });

        result.ShouldHaveValidationErrorFor(c => c.Type);
    }

    [Fact]
    public void Validate_WithInTypeAndSourceLocation_HasError()
    {
        var command = ValidInCommand() with { SourceLocationId = SourceLocationId };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.SourceLocationId);
    }

    [Fact]
    public void Validate_WithInTypeAndNoDestination_HasError()
    {
        var command = ValidInCommand() with { DestinationLocationId = null };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.DestinationLocationId);
    }

    [Fact]
    public void Validate_WithOutTypeAndSourceOnly_HasNoErrors()
    {
        var command = new CreateStockMovementCommand(
            ProductId, MovementType.Out, 10, SourceLocationId, DestinationLocationId: null, Direction: null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithOutTypeAndDestinationLocation_HasError()
    {
        var command = new CreateStockMovementCommand(
            ProductId, MovementType.Out, 10, SourceLocationId, DestinationLocationId, Direction: null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.DestinationLocationId);
    }

    [Fact]
    public void Validate_WithOutTypeAndNoSource_HasError()
    {
        var command = new CreateStockMovementCommand(
            ProductId, MovementType.Out, 10, SourceLocationId: null, DestinationLocationId: null, Direction: null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.SourceLocationId);
    }

    [Fact]
    public void Validate_WithTransferTypeAndBothLocations_HasNoErrors()
    {
        var command = new CreateStockMovementCommand(
            ProductId, MovementType.Transfer, 10, SourceLocationId, DestinationLocationId, Direction: null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithTransferTypeAndMissingSource_HasError()
    {
        var command = new CreateStockMovementCommand(
            ProductId, MovementType.Transfer, 10, SourceLocationId: null, DestinationLocationId, Direction: null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.SourceLocationId);
    }

    [Fact]
    public void Validate_WithTransferTypeAndMissingDestination_HasError()
    {
        var command = new CreateStockMovementCommand(
            ProductId, MovementType.Transfer, 10, SourceLocationId, DestinationLocationId: null, Direction: null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.DestinationLocationId);
    }

    [Fact]
    public void Validate_WithAdjustmentTypeDestinationAndDirection_HasNoErrors()
    {
        var command = new CreateStockMovementCommand(
            ProductId, MovementType.Adjustment, 10, SourceLocationId: null, DestinationLocationId, MovementDirection.Increase);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithAdjustmentTypeAndSourceLocation_HasError()
    {
        var command = new CreateStockMovementCommand(
            ProductId, MovementType.Adjustment, 10, SourceLocationId, DestinationLocationId, MovementDirection.Increase);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.SourceLocationId);
    }

    [Fact]
    public void Validate_WithAdjustmentTypeAndNoDirection_HasError()
    {
        var command = new CreateStockMovementCommand(
            ProductId, MovementType.Adjustment, 10, SourceLocationId: null, DestinationLocationId, Direction: null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Direction);
    }

    [Fact]
    public void Validate_WithNonAdjustmentTypeAndDirectionProvided_HasError()
    {
        var command = ValidInCommand() with { Direction = MovementDirection.Increase };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Direction);
    }
}
