using StockFlow.Domain.Common;
using StockFlow.Domain.StockMovements;

namespace StockFlow.Tests.Domain.StockMovements;

public class StockMovementTests
{
    private static readonly Guid ProductId = Guid.NewGuid();
    private static readonly Guid SourceLocationId = Guid.NewGuid();
    private static readonly Guid DestinationLocationId = Guid.NewGuid();

    [Fact]
    public void Create_WithInTypeAndDestinationOnly_Succeeds()
    {
        var movement = StockMovement.Create(ProductId, MovementType.In, 10, sourceLocationId: null, DestinationLocationId, direction: null);

        Assert.Equal(MovementType.In, movement.Type);
        Assert.Null(movement.SourceLocationId);
        Assert.Equal(DestinationLocationId, movement.DestinationLocationId);
        Assert.Null(movement.Direction);
    }

    [Fact]
    public void Create_WithInTypeAndSourceLocation_Throws()
    {
        Assert.Throws<DomainValidationException>(() =>
            StockMovement.Create(ProductId, MovementType.In, 10, SourceLocationId, DestinationLocationId, direction: null));
    }

    [Fact]
    public void Create_WithInTypeAndNoDestination_Throws()
    {
        Assert.Throws<DomainValidationException>(() =>
            StockMovement.Create(ProductId, MovementType.In, 10, sourceLocationId: null, destinationLocationId: null, direction: null));
    }

    [Fact]
    public void Create_WithOutTypeAndSourceOnly_Succeeds()
    {
        var movement = StockMovement.Create(ProductId, MovementType.Out, 10, SourceLocationId, destinationLocationId: null, direction: null);

        Assert.Equal(MovementType.Out, movement.Type);
        Assert.Equal(SourceLocationId, movement.SourceLocationId);
        Assert.Null(movement.DestinationLocationId);
    }

    [Fact]
    public void Create_WithOutTypeAndDestinationLocation_Throws()
    {
        Assert.Throws<DomainValidationException>(() =>
            StockMovement.Create(ProductId, MovementType.Out, 10, SourceLocationId, DestinationLocationId, direction: null));
    }

    [Fact]
    public void Create_WithOutTypeAndNoSource_Throws()
    {
        Assert.Throws<DomainValidationException>(() =>
            StockMovement.Create(ProductId, MovementType.Out, 10, sourceLocationId: null, destinationLocationId: null, direction: null));
    }

    [Fact]
    public void Create_WithTransferTypeAndBothLocations_Succeeds()
    {
        var movement = StockMovement.Create(ProductId, MovementType.Transfer, 10, SourceLocationId, DestinationLocationId, direction: null);

        Assert.Equal(SourceLocationId, movement.SourceLocationId);
        Assert.Equal(DestinationLocationId, movement.DestinationLocationId);
    }

    [Fact]
    public void Create_WithTransferTypeAndMissingSource_Throws()
    {
        Assert.Throws<DomainValidationException>(() =>
            StockMovement.Create(ProductId, MovementType.Transfer, 10, sourceLocationId: null, DestinationLocationId, direction: null));
    }

    [Fact]
    public void Create_WithTransferTypeAndMissingDestination_Throws()
    {
        Assert.Throws<DomainValidationException>(() =>
            StockMovement.Create(ProductId, MovementType.Transfer, 10, SourceLocationId, destinationLocationId: null, direction: null));
    }

    [Fact]
    public void Create_WithAdjustmentTypeAndDestinationAndDirection_Succeeds()
    {
        var movement = StockMovement.Create(
            ProductId, MovementType.Adjustment, 10, sourceLocationId: null, DestinationLocationId, MovementDirection.Increase);

        Assert.Equal(DestinationLocationId, movement.DestinationLocationId);
        Assert.Equal(MovementDirection.Increase, movement.Direction);
    }

    [Fact]
    public void Create_WithAdjustmentTypeAndSourceLocation_Throws()
    {
        Assert.Throws<DomainValidationException>(() =>
            StockMovement.Create(ProductId, MovementType.Adjustment, 10, SourceLocationId, DestinationLocationId, MovementDirection.Increase));
    }

    [Fact]
    public void Create_WithAdjustmentTypeAndNoDestination_Throws()
    {
        Assert.Throws<DomainValidationException>(() =>
            StockMovement.Create(ProductId, MovementType.Adjustment, 10, sourceLocationId: null, destinationLocationId: null, MovementDirection.Increase));
    }

    [Fact]
    public void Create_WithAdjustmentTypeAndNoDirection_Throws()
    {
        Assert.Throws<DomainValidationException>(() =>
            StockMovement.Create(ProductId, MovementType.Adjustment, 10, sourceLocationId: null, DestinationLocationId, direction: null));
    }

    [Fact]
    public void Create_WithNonAdjustmentTypeAndDirectionProvided_Throws()
    {
        Assert.Throws<DomainValidationException>(() =>
            StockMovement.Create(ProductId, MovementType.In, 10, sourceLocationId: null, DestinationLocationId, MovementDirection.Increase));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithZeroOrNegativeQuantity_Throws(int quantity)
    {
        Assert.Throws<DomainValidationException>(() =>
            StockMovement.Create(ProductId, MovementType.In, quantity, sourceLocationId: null, DestinationLocationId, direction: null));
    }

    [Fact]
    public void Create_SetsCreatedAtAndLeavesCreatedByUnset()
    {
        var movement = StockMovement.Create(ProductId, MovementType.In, 10, sourceLocationId: null, DestinationLocationId, direction: null);

        Assert.True(movement.CreatedAt <= DateTime.UtcNow);
        Assert.Null(movement.CreatedBy);
    }
}
