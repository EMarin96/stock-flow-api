using StockFlow.Domain.Common;
using StockFlow.Domain.StockMovements;

namespace StockFlow.Tests.Domain.StockMovements;

public class StockLevelTests
{
    private static readonly Guid ProductId = Guid.NewGuid();
    private static readonly Guid LocationId = Guid.NewGuid();

    [Fact]
    public void Create_StartsAtZeroQuantity()
    {
        var stockLevel = StockLevel.Create(ProductId, LocationId);

        Assert.Equal(0, stockLevel.Quantity);
        Assert.Equal(ProductId, stockLevel.ProductId);
        Assert.Equal(LocationId, stockLevel.LocationId);
    }

    [Fact]
    public void Increase_AddsAmountToQuantity()
    {
        var stockLevel = StockLevel.Create(ProductId, LocationId);

        stockLevel.Increase(10);

        Assert.Equal(10, stockLevel.Quantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Increase_WithZeroOrNegativeAmount_Throws(int amount)
    {
        var stockLevel = StockLevel.Create(ProductId, LocationId);

        Assert.Throws<DomainValidationException>(() => stockLevel.Increase(amount));
    }

    [Fact]
    public void Decrease_SubtractsAmountFromQuantity()
    {
        var stockLevel = StockLevel.Create(ProductId, LocationId);
        stockLevel.Increase(10);

        stockLevel.Decrease(4);

        Assert.Equal(6, stockLevel.Quantity);
    }

    [Fact]
    public void Decrease_ToExactlyZero_Succeeds()
    {
        var stockLevel = StockLevel.Create(ProductId, LocationId);
        stockLevel.Increase(10);

        stockLevel.Decrease(10);

        Assert.Equal(0, stockLevel.Quantity);
    }

    [Fact]
    public void Decrease_WhenResultWouldBeNegative_Throws()
    {
        var stockLevel = StockLevel.Create(ProductId, LocationId);
        stockLevel.Increase(5);

        Assert.Throws<DomainValidationException>(() => stockLevel.Decrease(6));
        Assert.Equal(5, stockLevel.Quantity); // unchanged after the failed decrease
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Decrease_WithZeroOrNegativeAmount_Throws(int amount)
    {
        var stockLevel = StockLevel.Create(ProductId, LocationId);
        stockLevel.Increase(10);

        Assert.Throws<DomainValidationException>(() => stockLevel.Decrease(amount));
    }
}
