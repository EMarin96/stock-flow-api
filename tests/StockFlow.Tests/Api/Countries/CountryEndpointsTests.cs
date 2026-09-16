using System.Net;
using System.Net.Http.Json;
using StockFlow.Application.Countries.Shared;
using StockFlow.Domain.Users;
using StockFlow.Tests.Api;

namespace StockFlow.Tests.Api.Countries;

[Collection("Postgres collection")]
public class CountryEndpointsTests : IClassFixture<ApiFactoryFixture>
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public CountryEndpointsTests(ApiFactoryFixture factoryFixture)
    {
        _factory = factoryFixture.Factory;
        _client = _factory.CreateAuthorizedClient(Role.Admin);
    }

    [Fact]
    public async Task GetCountries_ReturnsTheSupportedCountries()
    {
        var response = await _client.GetAsync("/api/countries");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var countries = await response.Content.ReadFromJsonAsync<List<CountryDto>>();
        Assert.Contains(countries!, country => country.Code == "US");
        Assert.Contains(countries!, country => country.Code == "CR");
    }

    [Fact]
    public async Task GetStates_ForASupportedCountry_ReturnsItsStates()
    {
        var response = await _client.GetAsync("/api/countries/US/states");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var states = await response.Content.ReadFromJsonAsync<List<StateDto>>();
        Assert.Contains(states!, state => state.Iso2 == "CA" && state.Name == "California");
    }

    [Fact]
    public async Task GetStates_WithAnUnsupportedCountryCode_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/api/countries/ZZ/states");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetCities_ForAValidCountryAndState_ReturnsItsCities()
    {
        var response = await _client.GetAsync("/api/countries/US/states/CA/cities");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cities = await response.Content.ReadFromJsonAsync<List<CityDto>>();
        Assert.Contains(cities!, city => city.Name == "Los Angeles");
    }

    [Fact]
    public async Task GetCities_WithAnUnrecognizedState_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/api/countries/US/states/ZZ/cities");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetStates_WithNoToken_ReturnsUnauthorized()
    {
        var anonymousClient = _factory.CreateClient();

        var response = await anonymousClient.GetAsync("/api/countries/US/states");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetStates_WithAReadOnlyToken_Succeeds()
    {
        var readOnlyClient = _factory.CreateAuthorizedClient(Role.ReadOnly);

        var response = await readOnlyClient.GetAsync("/api/countries/US/states");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
