using StockFlow.Domain.Common;
using StockFlow.Domain.Locations;

namespace StockFlow.Tests.Domain.Locations;

public class LocationCodeTests
{
    [Fact]
    public void Create_WithValidValue_CreatesLocationCode()
    {
        var code = LocationCode.Create("WH-001");

        Assert.Equal("WH-001", code.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WhenValueIsEmptyOrWhitespace_Throws(string value)
    {
        Assert.Throws<DomainValidationException>(() => LocationCode.Create(value));
    }

    [Fact]
    public void Create_WhenValueIsLongerThan32Characters_Throws()
    {
        var value = new string('a', 33);

        Assert.Throws<DomainValidationException>(() => LocationCode.Create(value));
    }

    [Fact]
    public void Create_WithValueExactly32Characters_DoesNotThrow()
    {
        var value = new string('a', 32);

        var code = LocationCode.Create(value);

        Assert.Equal(value, code.Value);
    }
}
