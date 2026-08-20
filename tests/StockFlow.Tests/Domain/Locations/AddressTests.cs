using StockFlow.Domain.Common;
using StockFlow.Domain.Locations;

namespace StockFlow.Tests.Domain.Locations;

public class AddressTests
{
    [Fact]
    public void Create_WithValidCoreTrio_CreatesAddress()
    {
        var address = Address.Create("123 Main St", null, null, "CA", "Los Angeles", Country.US);

        Assert.Equal("123 Main St", address.AddressLine1);
        Assert.Null(address.AddressLine2);
        Assert.Null(address.AddressLine3);
        Assert.Equal("CA", address.State);
        Assert.Equal("Los Angeles", address.City);
        Assert.Equal(Country.US, address.Country);
    }

    [Fact]
    public void Create_WithAllThreeAddressLines_CreatesAddress()
    {
        var address = Address.Create("123 Main St", "Suite 4", "Building B", "CA", "Los Angeles", Country.US);

        Assert.Equal("123 Main St", address.AddressLine1);
        Assert.Equal("Suite 4", address.AddressLine2);
        Assert.Equal("Building B", address.AddressLine3);
    }

    [Fact]
    public void Create_WithoutAnyAddressLines_CreatesAddress()
    {
        var address = Address.Create(null, null, null, "CA", "Los Angeles", Country.US);

        Assert.Null(address.AddressLine1);
        Assert.Null(address.AddressLine2);
        Assert.Null(address.AddressLine3);
        Assert.Equal("CA", address.State);
        Assert.Equal("Los Angeles", address.City);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WhenStateIsMissing_Throws(string? state)
    {
        Assert.Throws<DomainValidationException>(() => Address.Create(null, null, null, state!, "Los Angeles", Country.US));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WhenCityIsMissing_Throws(string? city)
    {
        Assert.Throws<DomainValidationException>(() => Address.Create(null, null, null, "CA", city!, Country.US));
    }

    [Fact]
    public void Create_WhenAddressLine1IsLongerThan200Characters_Throws()
    {
        var line = new string('a', 201);

        Assert.Throws<DomainValidationException>(() => Address.Create(line, null, null, "CA", "Los Angeles", Country.US));
    }

    [Fact]
    public void Create_WhenAddressLine2IsLongerThan200Characters_Throws()
    {
        var line = new string('a', 201);

        Assert.Throws<DomainValidationException>(() => Address.Create(null, line, null, "CA", "Los Angeles", Country.US));
    }

    [Fact]
    public void Create_WhenAddressLine3IsLongerThan200Characters_Throws()
    {
        var line = new string('a', 201);

        Assert.Throws<DomainValidationException>(() => Address.Create(null, null, line, "CA", "Los Angeles", Country.US));
    }

    [Fact]
    public void Create_WhenStateIsLongerThan10Characters_Throws()
    {
        var state = new string('a', 11);

        Assert.Throws<DomainValidationException>(() => Address.Create(null, null, null, state, "Los Angeles", Country.US));
    }

    [Fact]
    public void Create_WhenCityIsLongerThan100Characters_Throws()
    {
        var city = new string('a', 101);

        Assert.Throws<DomainValidationException>(() => Address.Create(null, null, null, "CA", city, Country.US));
    }
}
