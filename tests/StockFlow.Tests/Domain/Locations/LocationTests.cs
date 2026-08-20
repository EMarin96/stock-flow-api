using StockFlow.Domain.Common;
using StockFlow.Domain.Locations;

namespace StockFlow.Tests.Domain.Locations;

public class LocationTests
{
    [Fact]
    public void Create_WithFullTrioAndAddressLines_Succeeds()
    {
        var location = Location.Create(
            "WH-100",
            "Main Warehouse",
            addressLine1: "123 Main St",
            addressLine2: "Suite 4",
            addressLine3: null,
            state: "CA",
            city: "Los Angeles",
            country: Country.US);

        Assert.NotNull(location.Address);
        Assert.Equal("123 Main St", location.Address.AddressLine1);
        Assert.Equal("Suite 4", location.Address.AddressLine2);
        Assert.Equal("CA", location.Address.State);
        Assert.Equal("Los Angeles", location.Address.City);
        Assert.Equal(Country.US, location.Address.Country);
    }

    [Fact]
    public void Create_WithFullTrioAndNoAddressLines_Succeeds()
    {
        var location = Location.Create(
            "WH-100",
            "Main Warehouse",
            addressLine1: null,
            addressLine2: null,
            addressLine3: null,
            state: "CA",
            city: "Los Angeles",
            country: Country.US);

        Assert.NotNull(location.Address);
        Assert.Null(location.Address.AddressLine1);
        Assert.Equal("CA", location.Address.State);
        Assert.Equal("Los Angeles", location.Address.City);
    }

    [Fact]
    public void Create_WhenStateIsMissing_Throws()
    {
        Assert.Throws<DomainValidationException>(() => Location.Create(
            "WH-100",
            "Main Warehouse",
            addressLine1: null,
            addressLine2: null,
            addressLine3: null,
            state: "",
            city: "Los Angeles",
            country: Country.US));
    }

    [Fact]
    public void Create_WhenCityIsMissing_Throws()
    {
        Assert.Throws<DomainValidationException>(() => Location.Create(
            "WH-100",
            "Main Warehouse",
            addressLine1: null,
            addressLine2: null,
            addressLine3: null,
            state: "CA",
            city: "",
            country: Country.US));
    }

    [Fact]
    public void Update_WithFullTrioAndAddressLines_Succeeds()
    {
        var location = Location.Create("WH-100", "Main Warehouse", null, null, null, "CA", "Los Angeles", Country.US);

        location.Update(
            "Main Warehouse",
            addressLine1: "123 Main St",
            addressLine2: null,
            addressLine3: null,
            state: "CA",
            city: "Los Angeles",
            country: Country.US);

        Assert.NotNull(location.Address);
        Assert.Equal("123 Main St", location.Address.AddressLine1);
        Assert.Equal("CA", location.Address.State);
        Assert.Equal("Los Angeles", location.Address.City);
    }

    [Fact]
    public void Update_WhenStateIsMissing_Throws()
    {
        var location = Location.Create("WH-100", "Main Warehouse", null, null, null, "CA", "Los Angeles", Country.US);

        Assert.Throws<DomainValidationException>(() => location.Update(
            "Main Warehouse",
            addressLine1: null,
            addressLine2: null,
            addressLine3: null,
            state: "",
            city: "Los Angeles",
            country: Country.US));
    }
}
