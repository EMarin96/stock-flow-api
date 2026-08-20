using StockFlow.Domain.Common;
using StockFlow.Domain.Products;

namespace StockFlow.Tests.Domain.Products;

public class SkuTests
{
    [Fact]
    public void Create_WithValidValue_CreatesSku()
    {
        var sku = Sku.Create("SKU-100");

        Assert.Equal("SKU-100", sku.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WhenValueIsEmptyOrWhitespace_Throws(string value)
    {
        Assert.Throws<DomainValidationException>(() => Sku.Create(value));
    }

    [Fact]
    public void Create_WhenValueIsLongerThan64Characters_Throws()
    {
        var value = new string('a', 65);

        Assert.Throws<DomainValidationException>(() => Sku.Create(value));
    }

    [Fact]
    public void Create_WithValueExactly64Characters_DoesNotThrow()
    {
        var value = new string('a', 64);

        var sku = Sku.Create(value);

        Assert.Equal(value, sku.Value);
    }
}
