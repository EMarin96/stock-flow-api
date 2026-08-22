using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using StockFlow.Api.Endpoints;
using StockFlow.Application.Common.Pagination;
using StockFlow.Application.Locations.Shared;
using StockFlow.Domain.Locations;
using StockFlow.Tests.Api;

namespace StockFlow.Tests.Api.Locations;

[Collection("Postgres collection")]
public class LocationEndpointsTests : IClassFixture<ApiFactoryFixture>, IAsyncLifetime
{
    // Mirrors the server's JsonStringEnumConverter registration (Program.cs), so
    // the test client can round-trip the Country enum as "US"/"CR" strings too.
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public LocationEndpointsTests(ApiFactoryFixture factoryFixture)
    {
        _factory = factoryFixture.Factory;
        _client = _factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    // Matches the seed data in InMemoryCountryReferenceDataService (US -> CA -> "Los Angeles").
    private static CreateLocationRequest ValidRequest(string code = "WH-100") =>
        new(code, "Main Warehouse", "123 Main St", null, null, "CA", "Los Angeles", Country.US);

    [Fact]
    public async Task Create_ThenGetById_ReturnsTheCreatedLocation()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/locations", ValidRequest());
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<LocationDto>(JsonOptions);
        Assert.NotNull(created);

        var getResponse = await _client.GetAsync($"/api/locations/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<LocationDto>(JsonOptions);
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal("WH-100", fetched.Code);
        Assert.Equal("Los Angeles", fetched.City);
        Assert.Equal("CA", fetched.State);
        Assert.Equal(Country.US, fetched.Country);
    }

    [Fact]
    public async Task Create_WithDuplicateCode_ReturnsConflict()
    {
        var firstResponse = await _client.PostAsJsonAsync("/api/locations", ValidRequest("WH-DUP"));
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        // Exercises both the pre-check and (transitively) the DB unique-constraint
        // translation path — either way the client must see the same Conflict.
        var secondResponse = await _client.PostAsJsonAsync("/api/locations", ValidRequest("WH-DUP"));

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Create_WithEmptyCode_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/locations",
            ValidRequest() with { Code = string.Empty });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithAddressLineButMissingState_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/locations",
            ValidRequest("WH-NO-STATE") with { State = string.Empty });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithAddressLineButMissingCity_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/locations",
            ValidRequest("WH-NO-CITY") with { City = string.Empty });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithFullTrioAndOptionalAddressLines_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/locations",
            ValidRequest("WH-FULL-TRIO-LINES") with { AddressLine2 = "Suite 4", AddressLine3 = "Building B" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<LocationDto>(JsonOptions);
        Assert.Equal("123 Main St", created!.AddressLine1);
        Assert.Equal("Suite 4", created.AddressLine2);
        Assert.Equal("Building B", created.AddressLine3);
        Assert.Equal("CA", created.State);
        Assert.Equal("Los Angeles", created.City);
        Assert.Equal(Country.US, created.Country);
    }

    [Fact]
    public async Task Create_WithFullTrioAndNoAddressLines_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/locations",
            ValidRequest("WH-FULL-TRIO-NO-LINES") with { AddressLine1 = null, AddressLine2 = null, AddressLine3 = null });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<LocationDto>(JsonOptions);
        Assert.Null(created!.AddressLine1);
        Assert.Equal("CA", created.State);
        Assert.Equal("Los Angeles", created.City);
        Assert.Equal(Country.US, created.Country);
    }

    [Fact]
    public async Task Create_WithUnrecognizedState_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/locations",
            ValidRequest("WH-BAD-STATE") with { State = "ZZ" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithUnrecognizedCity_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/locations",
            ValidRequest("WH-BAD-CITY") with { City = "Nowhere" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ForUnknownId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/locations/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_ChangesEditableFields_ButNotTheCode()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/locations", ValidRequest("WH-UPDATE"));
        var created = await createResponse.Content.ReadFromJsonAsync<LocationDto>(JsonOptions);

        var updateRequest = new UpdateLocationRequest("Main Warehouse v2", "456 Other St", null, null, "NY", "New York City", Country.US);
        var updateResponse = await _client.PutAsJsonAsync($"/api/locations/{created!.Id}", updateRequest);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<LocationDto>(JsonOptions);
        Assert.Equal("Main Warehouse v2", updated!.Name);
        Assert.Equal("WH-UPDATE", updated.Code);
        Assert.Equal("New York City", updated.City);
    }

    [Fact]
    public async Task Update_ForUnknownId_ReturnsNotFound()
    {
        var updateRequest = new UpdateLocationRequest("Main Warehouse", null, null, null, "CA", "Los Angeles", Country.US);

        var response = await _client.PutAsJsonAsync($"/api/locations/{Guid.NewGuid()}", updateRequest);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithFullTrioAndNoAddressLines_ReturnsOk()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/locations", ValidRequest("WH-UPDATE-NO-LINES"));
        var created = await createResponse.Content.ReadFromJsonAsync<LocationDto>(JsonOptions);

        var updateRequest = new UpdateLocationRequest("Main Warehouse", null, null, null, "CA", "Los Angeles", Country.US);
        var response = await _client.PutAsJsonAsync($"/api/locations/{created!.Id}", updateRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<LocationDto>(JsonOptions);
        Assert.Null(updated!.AddressLine1);
        Assert.Equal("CA", updated.State);
        Assert.Equal("Los Angeles", updated.City);
    }

    [Fact]
    public async Task Update_WithMissingState_ReturnsBadRequest()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/locations", ValidRequest("WH-UPDATE-NO-STATE"));
        var created = await createResponse.Content.ReadFromJsonAsync<LocationDto>(JsonOptions);

        var updateRequest = new UpdateLocationRequest("Main Warehouse", "123 Main St", null, null, string.Empty, "Los Angeles", Country.US);
        var response = await _client.PutAsJsonAsync($"/api/locations/{created!.Id}", updateRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delete_SoftDeletes_SubsequentGetAndDeleteReturnNotFound()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/locations", ValidRequest("WH-DELETE"));
        var created = await createResponse.Content.ReadFromJsonAsync<LocationDto>(JsonOptions);

        var deleteResponse = await _client.DeleteAsync($"/api/locations/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/locations/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

        var secondDeleteResponse = await _client.DeleteAsync($"/api/locations/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, secondDeleteResponse.StatusCode);
    }

    [Fact]
    public async Task List_WithPagination_ExcludesSoftDeletedLocations()
    {
        var toDelete = await _client.PostAsJsonAsync("/api/locations", ValidRequest("WH-LIST-DELETED"));
        var deletedLocation = await toDelete.Content.ReadFromJsonAsync<LocationDto>(JsonOptions);
        await _client.DeleteAsync($"/api/locations/{deletedLocation!.Id}");

        for (var i = 0; i < 3; i++)
        {
            await _client.PostAsJsonAsync("/api/locations", ValidRequest($"WH-LIST-{i}"));
        }

        var response = await _client.GetAsync("/api/locations?page=1&pageSize=2");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var page = await response.Content.ReadFromJsonAsync<PagedResult<LocationDto>>(JsonOptions);
        Assert.NotNull(page);
        Assert.Equal(2, page!.Items.Count);
        Assert.Equal(3, page.TotalCount); // the soft-deleted location must not be counted
        Assert.DoesNotContain(page.Items, item => item.Id == deletedLocation.Id);
    }

    [Fact]
    public async Task List_WithInvalidPageSize_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/api/locations?page=1&pageSize=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
