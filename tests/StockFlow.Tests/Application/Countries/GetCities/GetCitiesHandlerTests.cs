using StockFlow.Application.Countries.GetCities;
using StockFlow.Application.Locations.Shared;

namespace StockFlow.Tests.Application.Countries.GetCities;

public class GetCitiesHandlerTests
{
    [Fact]
    public async Task GetCities_ForAValidCountryAndState_ReturnsItsCities()
    {
        var handler = new GetCitiesHandler(new InMemoryCountryReferenceDataService());

        var result = await handler.Handle(new GetCitiesQuery("US", "CA"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value, city => city.Name == "Los Angeles");
    }

    [Fact]
    public async Task GetCities_StateCodeIsCaseInsensitive()
    {
        var handler = new GetCitiesHandler(new InMemoryCountryReferenceDataService());

        var result = await handler.Handle(new GetCitiesQuery("US", "ca"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value);
    }

    [Fact]
    public async Task GetCities_WithAnUnsupportedCountryCode_ReturnsValidationError()
    {
        var handler = new GetCitiesHandler(new InMemoryCountryReferenceDataService());

        var result = await handler.Handle(new GetCitiesQuery("ZZ", "CA"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal("Countries.InvalidCountryCode", result.Error.Code);
    }

    [Fact]
    public async Task GetCities_WithAnUnrecognizedState_ReturnsValidationError()
    {
        var handler = new GetCitiesHandler(new InMemoryCountryReferenceDataService());

        var result = await handler.Handle(new GetCitiesQuery("US", "ZZ"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal("Countries.InvalidState", result.Error.Code);
    }

    [Fact]
    public async Task GetCities_WhenStatesLookupIsUnavailable_ReturnsUnavailableError()
    {
        var handler = new GetCitiesHandler(new UnavailableCountryReferenceDataService());

        var result = await handler.Handle(new GetCitiesQuery("US", "CA"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unavailable, result.Error.Type);
        Assert.Equal("Countries.ReferenceDataUnavailable", result.Error.Code);
    }
}
