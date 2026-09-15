using StockFlow.Application.Countries.GetStates;
using StockFlow.Application.Locations.Shared;

namespace StockFlow.Tests.Application.Countries.GetStates;

public class GetStatesHandlerTests
{
    [Fact]
    public async Task GetStates_ForASupportedCountry_ReturnsItsStates()
    {
        var handler = new GetStatesHandler(new InMemoryCountryReferenceDataService());

        var result = await handler.Handle(new GetStatesQuery("US"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value, state => state.Iso2 == "CA" && state.Name == "California");
    }

    [Fact]
    public async Task GetStates_CountryCodeIsCaseInsensitive()
    {
        var handler = new GetStatesHandler(new InMemoryCountryReferenceDataService());

        var result = await handler.Handle(new GetStatesQuery("us"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value);
    }

    [Fact]
    public async Task GetStates_WithAnUnsupportedCountryCode_ReturnsValidationError()
    {
        var handler = new GetStatesHandler(new InMemoryCountryReferenceDataService());

        var result = await handler.Handle(new GetStatesQuery("ZZ"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal("Countries.InvalidCountryCode", result.Error.Code);
    }

    [Fact]
    public async Task GetStates_WhenReferenceDataServiceIsUnavailable_ReturnsUnavailableError()
    {
        var handler = new GetStatesHandler(new UnavailableCountryReferenceDataService());

        var result = await handler.Handle(new GetStatesQuery("US"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unavailable, result.Error.Type);
        Assert.Equal("Countries.ReferenceDataUnavailable", result.Error.Code);
    }
}
