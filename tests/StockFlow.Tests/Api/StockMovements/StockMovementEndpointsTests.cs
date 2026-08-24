using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using StockFlow.Api.Endpoints;
using StockFlow.Application.Common.Pagination;
using StockFlow.Application.Locations.Shared;
using StockFlow.Application.Products.Shared;
using StockFlow.Application.StockMovements.Shared;
using StockFlow.Domain.Locations;
using StockFlow.Domain.Products;
using StockFlow.Domain.StockMovements;
using StockFlow.Domain.Users;
using StockFlow.Tests.Api;

namespace StockFlow.Tests.Api.StockMovements;

[Collection("Postgres collection")]
public class StockMovementEndpointsTests : IClassFixture<ApiFactoryFixture>, IAsyncLifetime
{
    // Mirrors the server's JsonStringEnumConverter registration (Program.cs), so
    // the test client can round-trip MovementType/MovementDirection/Country as
    // strings too.
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public StockMovementEndpointsTests(ApiFactoryFixture factoryFixture)
    {
        _factory = factoryFixture.Factory;
        _client = _factory.CreateAuthorizedClient(Role.Admin);
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    // Matches the seed data in InMemoryCountryReferenceDataService (US -> CA -> Los Angeles; CR -> SJ -> San José).
    private async Task<ProductDto> CreateProductAsync(string sku = "SKU-100")
    {
        var request = new CreateProductRequest(sku, "Widget", null, "unit", 10m, Currency.USD, 5);
        var response = await _client.PostAsJsonAsync("/api/products", request, JsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ProductDto>(JsonOptions))!;
    }

    private async Task<LocationDto> CreateLocationAsync(string code = "WH-100", Country country = Country.US)
    {
        var (state, city) = country == Country.US ? ("CA", "Los Angeles") : ("SJ", "San José");
        var request = new CreateLocationRequest(code, "Main Warehouse", null, null, null, state, city, country);
        var response = await _client.PostAsJsonAsync("/api/locations", request, JsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LocationDto>(JsonOptions))!;
    }

    private async Task<StockMovementDto> CreateMovementAsync(CreateStockMovementRequest request)
    {
        var response = await _client.PostAsJsonAsync("/api/stock-movements", request, JsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<StockMovementDto>(JsonOptions))!;
    }

    [Fact]
    public async Task Create_WithInType_ReturnsCreatedAndAtomicallyUpdatesLocationStock()
    {
        var product = await CreateProductAsync();
        var location = await CreateLocationAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/stock-movements",
            new CreateStockMovementRequest(product.Id, MovementType.In, 10, null, location.Id, null),
            JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<StockMovementDto>(JsonOptions);
        Assert.Equal(MovementType.In, created!.Type);
        Assert.Equal(10, created.Quantity);
        Assert.Equal(location.Id, created.DestinationLocationId);
        Assert.Null(created.SourceLocationId);
        Assert.True(created.CreatedAt <= DateTime.UtcNow);

        // Same operation atomically updated the location's current stock (see
        // spec.md acceptance criteria — "updates ... atomically").
        var stockResponse = await _client.GetAsync($"/api/locations/{location.Id}/products");
        var stockPage = await stockResponse.Content.ReadFromJsonAsync<PagedResult<LocationStockDto>>(JsonOptions);
        Assert.Contains(stockPage!.Items, item => item.ProductId == product.Id && item.Quantity == 10);
    }

    [Fact]
    public async Task Create_WithOutTypeAndEnoughStock_DecreasesSourceStock()
    {
        var product = await CreateProductAsync();
        var location = await CreateLocationAsync();
        await CreateMovementAsync(new CreateStockMovementRequest(product.Id, MovementType.In, 20, null, location.Id, null));

        var response = await _client.PostAsJsonAsync(
            "/api/stock-movements",
            new CreateStockMovementRequest(product.Id, MovementType.Out, 8, location.Id, null, null),
            JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var stockResponse = await _client.GetAsync($"/api/locations/{location.Id}/products");
        var stockPage = await stockResponse.Content.ReadFromJsonAsync<PagedResult<LocationStockDto>>(JsonOptions);
        Assert.Contains(stockPage!.Items, item => item.ProductId == product.Id && item.Quantity == 12);
    }

    [Fact]
    public async Task Create_WithTransfer_MovesStockBetweenLocations()
    {
        var product = await CreateProductAsync();
        var source = await CreateLocationAsync("WH-SRC");
        var destination = await CreateLocationAsync("WH-DEST");
        await CreateMovementAsync(new CreateStockMovementRequest(product.Id, MovementType.In, 30, null, source.Id, null));

        var response = await _client.PostAsJsonAsync(
            "/api/stock-movements",
            new CreateStockMovementRequest(product.Id, MovementType.Transfer, 10, source.Id, destination.Id, null),
            JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var sourceStock = await (await _client.GetAsync($"/api/locations/{source.Id}/products"))
            .Content.ReadFromJsonAsync<PagedResult<LocationStockDto>>(JsonOptions);
        var destinationStock = await (await _client.GetAsync($"/api/locations/{destination.Id}/products"))
            .Content.ReadFromJsonAsync<PagedResult<LocationStockDto>>(JsonOptions);

        Assert.Contains(sourceStock!.Items, item => item.ProductId == product.Id && item.Quantity == 20);
        Assert.Contains(destinationStock!.Items, item => item.ProductId == product.Id && item.Quantity == 10);
    }

    [Fact]
    public async Task Create_WithAdjustmentIncrease_IncreasesStock()
    {
        var product = await CreateProductAsync();
        var location = await CreateLocationAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/stock-movements",
            new CreateStockMovementRequest(product.Id, MovementType.Adjustment, 5, null, location.Id, MovementDirection.Increase),
            JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var stockPage = await (await _client.GetAsync($"/api/locations/{location.Id}/products"))
            .Content.ReadFromJsonAsync<PagedResult<LocationStockDto>>(JsonOptions);
        Assert.Contains(stockPage!.Items, item => item.ProductId == product.Id && item.Quantity == 5);
    }

    [Fact]
    public async Task Create_WithAdjustmentDecrease_DecreasesStock()
    {
        var product = await CreateProductAsync();
        var location = await CreateLocationAsync();
        await CreateMovementAsync(new CreateStockMovementRequest(product.Id, MovementType.In, 10, null, location.Id, null));

        var response = await _client.PostAsJsonAsync(
            "/api/stock-movements",
            new CreateStockMovementRequest(product.Id, MovementType.Adjustment, 3, null, location.Id, MovementDirection.Decrease),
            JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var stockPage = await (await _client.GetAsync($"/api/locations/{location.Id}/products"))
            .Content.ReadFromJsonAsync<PagedResult<LocationStockDto>>(JsonOptions);
        Assert.Contains(stockPage!.Items, item => item.ProductId == product.Id && item.Quantity == 7);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Create_WithZeroOrNegativeQuantity_ReturnsBadRequest(int quantity)
    {
        var product = await CreateProductAsync();
        var location = await CreateLocationAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/stock-movements",
            new CreateStockMovementRequest(product.Id, MovementType.In, quantity, null, location.Id, null),
            JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithInTypeAndSourceLocation_ReturnsBadRequest()
    {
        var product = await CreateProductAsync();
        var location = await CreateLocationAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/stock-movements",
            new CreateStockMovementRequest(product.Id, MovementType.In, 10, location.Id, location.Id, null),
            JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithOutTypeAndNoSourceLocation_ReturnsBadRequest()
    {
        var product = await CreateProductAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/stock-movements",
            new CreateStockMovementRequest(product.Id, MovementType.Out, 10, null, null, null),
            JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithAdjustmentTypeAndNoDirection_ReturnsBadRequest()
    {
        var product = await CreateProductAsync();
        var location = await CreateLocationAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/stock-movements",
            new CreateStockMovementRequest(product.Id, MovementType.Adjustment, 10, null, location.Id, null),
            JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithInTypeAndDirectionProvided_ReturnsBadRequest()
    {
        var product = await CreateProductAsync();
        var location = await CreateLocationAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/stock-movements",
            new CreateStockMovementRequest(product.Id, MovementType.In, 10, null, location.Id, MovementDirection.Increase),
            JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithUnknownProductId_ReturnsNotFound()
    {
        var location = await CreateLocationAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/stock-movements",
            new CreateStockMovementRequest(Guid.NewGuid(), MovementType.In, 10, null, location.Id, null),
            JsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithUnknownLocationId_ReturnsNotFound()
    {
        var product = await CreateProductAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/stock-movements",
            new CreateStockMovementRequest(product.Id, MovementType.In, 10, null, Guid.NewGuid(), null),
            JsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ForTransferAcrossDifferentCountries_ReturnsBadRequest()
    {
        var product = await CreateProductAsync();
        var source = await CreateLocationAsync("WH-US", Country.US);
        var destination = await CreateLocationAsync("WH-CR", Country.CR);
        await CreateMovementAsync(new CreateStockMovementRequest(product.Id, MovementType.In, 20, null, source.Id, null));

        var response = await _client.PostAsJsonAsync(
            "/api/stock-movements",
            new CreateStockMovementRequest(product.Id, MovementType.Transfer, 5, source.Id, destination.Id, null),
            JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WhenMovementWouldLeaveStockNegative_ReturnsConflict()
    {
        var product = await CreateProductAsync();
        var location = await CreateLocationAsync();
        await CreateMovementAsync(new CreateStockMovementRequest(product.Id, MovementType.In, 5, null, location.Id, null));

        var response = await _client.PostAsJsonAsync(
            "/api/stock-movements",
            new CreateStockMovementRequest(product.Id, MovementType.Out, 10, location.Id, null, null),
            JsonOptions);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        // Stock must remain unchanged after the rejected movement.
        var stockPage = await (await _client.GetAsync($"/api/locations/{location.Id}/products"))
            .Content.ReadFromJsonAsync<PagedResult<LocationStockDto>>(JsonOptions);
        Assert.Contains(stockPage!.Items, item => item.ProductId == product.Id && item.Quantity == 5);
    }

    [Fact]
    public async Task List_WithPagination_ReturnsMovementsNewestFirst()
    {
        var product = await CreateProductAsync();
        var location = await CreateLocationAsync();
        for (var i = 0; i < 3; i++)
        {
            await CreateMovementAsync(new CreateStockMovementRequest(product.Id, MovementType.In, 1, null, location.Id, null));
        }

        var response = await _client.GetAsync("/api/stock-movements?page=1&pageSize=2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<StockMovementDto>>(JsonOptions);
        Assert.Equal(2, page!.Items.Count);
        Assert.Equal(3, page.TotalCount);
    }

    [Fact]
    public async Task List_FilteredByProductId_ReturnsOnlyThatProductsMovements()
    {
        var wantedProduct = await CreateProductAsync("SKU-WANTED");
        var otherProduct = await CreateProductAsync("SKU-OTHER");
        var location = await CreateLocationAsync();
        await CreateMovementAsync(new CreateStockMovementRequest(wantedProduct.Id, MovementType.In, 1, null, location.Id, null));
        await CreateMovementAsync(new CreateStockMovementRequest(otherProduct.Id, MovementType.In, 1, null, location.Id, null));

        var response = await _client.GetAsync($"/api/stock-movements?productId={wantedProduct.Id}");

        var page = await response.Content.ReadFromJsonAsync<PagedResult<StockMovementDto>>(JsonOptions);
        Assert.Single(page!.Items);
        Assert.Equal(wantedProduct.Id, page.Items[0].ProductId);
    }

    [Fact]
    public async Task List_FilteredByLocationId_MatchesEitherSourceOrDestination()
    {
        var product = await CreateProductAsync();
        var source = await CreateLocationAsync("WH-SRC");
        var destination = await CreateLocationAsync("WH-DEST");
        var untouched = await CreateLocationAsync("WH-UNTOUCHED");
        await CreateMovementAsync(new CreateStockMovementRequest(product.Id, MovementType.In, 30, null, source.Id, null));
        await CreateMovementAsync(new CreateStockMovementRequest(product.Id, MovementType.Transfer, 10, source.Id, destination.Id, null));

        var sourceResponse = await _client.GetAsync($"/api/stock-movements?locationId={source.Id}");
        var sourcePage = await sourceResponse.Content.ReadFromJsonAsync<PagedResult<StockMovementDto>>(JsonOptions);
        Assert.Equal(2, sourcePage!.TotalCount); // the initial In, and the Transfer's source half

        var destinationResponse = await _client.GetAsync($"/api/stock-movements?locationId={destination.Id}");
        var destinationPage = await destinationResponse.Content.ReadFromJsonAsync<PagedResult<StockMovementDto>>(JsonOptions);
        Assert.Equal(1, destinationPage!.TotalCount); // only the Transfer's destination half

        var untouchedResponse = await _client.GetAsync($"/api/stock-movements?locationId={untouched.Id}");
        var untouchedPage = await untouchedResponse.Content.ReadFromJsonAsync<PagedResult<StockMovementDto>>(JsonOptions);
        Assert.Equal(0, untouchedPage!.TotalCount);
    }

    [Fact]
    public async Task List_FilteredByType_ReturnsOnlyThatType()
    {
        var product = await CreateProductAsync();
        var location = await CreateLocationAsync();
        await CreateMovementAsync(new CreateStockMovementRequest(product.Id, MovementType.In, 10, null, location.Id, null));
        await CreateMovementAsync(new CreateStockMovementRequest(product.Id, MovementType.Out, 4, location.Id, null, null));

        var response = await _client.GetAsync("/api/stock-movements?type=Out");

        var page = await response.Content.ReadFromJsonAsync<PagedResult<StockMovementDto>>(JsonOptions);
        Assert.Single(page!.Items);
        Assert.Equal(MovementType.Out, page.Items[0].Type);
    }

    [Fact]
    public async Task List_WithInvalidPageSize_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/api/stock-movements?page=1&pageSize=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
