using StockFlow.Application.Countries.GetCountries;
using StockFlow.Domain.Locations;

namespace StockFlow.Tests.Application.Countries.GetCountries;

public class GetCountriesHandlerTests
{
    [Fact]
    public async Task GetCountries_ReturnsEveryCountryStockFlowSupports()
    {
        var handler = new GetCountriesHandler();

        var result = await handler.Handle(new GetCountriesQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var codes = result.Value.Select(country => country.Code).ToList();
        Assert.Equal(Enum.GetValues<Country>().Select(country => country.ToString()), codes);
    }
}
