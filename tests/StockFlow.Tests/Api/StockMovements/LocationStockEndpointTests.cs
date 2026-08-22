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
using StockFlow.Tests.Api;

namespace StockFlow.Tests.Api.StockMovements;

/// <summary>
/// Covers GET /api/locations/{locationId}/products — lives here (not under
/// Api/Locations) because it needs Product + StockMovement fixtures too (see
/// plan.md — Implementation, step 16, for why the endpoint itself lives in
/// LocationEndpoints.cs while remaining conceptually part of this feature).
/// The shared ApiFactory truncates every table regardless, so no factory
/// distinction is needed here anymore.
/// </summary>
[Collection("Postgres collection")]
public class LocationStockEndpointTests : IClassFixture<ApiFactoryFixture>, IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public LocationStockEndpointTests(ApiFactoryFixture factoryFixture)
    {
        _factory = factoryFixture.Factory;
        _client = _factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<ProductDto> CreateProductAsync(string sku, string name)
    {
        var request = new CreateProductRequest(sku, name, null, "unit", 10m, Currency.USD, 5);
        var response = await _client.PostAsJsonAsync("/api/products", request, JsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ProductDto>(JsonOptions))!;
    }

    private async Task<LocationDto> CreateLocationAsync(string code = "WH-100")
    {
        var request = new CreateLocationRequest(code, "Main Warehouse", null, null, null, "CA", "Los Angeles", Country.US);
        var response = await _client.PostAsJsonAsync("/api/locations", request, JsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LocationDto>(JsonOptions))!;
    }

    private async Task CreateInMovementAsync(Guid productId, Guid destinationLocationId, int quantity) =>
        (await _client.PostAsJsonAsync(
            "/api/stock-movements",
            new CreateStockMovementRequest(productId, MovementType.In, quantity, null, destinationLocationId, null),
            JsonOptions)).EnsureSuccessStatusCode();

    [Fact]
    public async Task GetLocationStock_IncludesProductsWithZeroStock()
    {
        var location = await CreateLocationAsync();
        var stockedProduct = await CreateProductAsync("SKU-STOCKED", "Widget");
        var zeroStockProduct = await CreateProductAsync("SKU-ZERO", "Gadget");
        await CreateInMovementAsync(stockedProduct.Id, location.Id, 10);

        var response = await _client.GetAsync($"/api/locations/{location.Id}/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<LocationStockDto>>(JsonOptions);
        Assert.Contains(page!.Items, item => item.ProductId == stockedProduct.Id && item.Quantity == 10);
        Assert.Contains(page.Items, item => item.ProductId == zeroStockProduct.Id && item.Quantity == 0);
    }

    [Fact]
    public async Task GetLocationStock_FilteredByProductName_ReturnsOnlyMatchingProducts()
    {
        var location = await CreateLocationAsync();
        await CreateProductAsync("SKU-1", "Widget");
        await CreateProductAsync("SKU-2", "Gadget");

        var response = await _client.GetAsync($"/api/locations/{location.Id}/products?productName=Widg");

        var page = await response.Content.ReadFromJsonAsync<PagedResult<LocationStockDto>>(JsonOptions);
        Assert.Single(page!.Items);
        Assert.Equal("Widget", page.Items[0].ProductName);
    }

    [Fact]
    public async Task GetLocationStock_WithPagination_ReturnsRequestedPageSize()
    {
        var location = await CreateLocationAsync();
        for (var i = 0; i < 3; i++)
        {
            await CreateProductAsync($"SKU-{i}", $"Product {i}");
        }

        var response = await _client.GetAsync($"/api/locations/{location.Id}/products?page=1&pageSize=2");

        var page = await response.Content.ReadFromJsonAsync<PagedResult<LocationStockDto>>(JsonOptions);
        Assert.Equal(2, page!.Items.Count);
        Assert.Equal(3, page.TotalCount);
    }

    [Fact]
    public async Task GetLocationStock_ForUnknownLocation_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/locations/{Guid.NewGuid()}/products");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetLocationStock_WithInvalidPageSize_ReturnsBadRequest()
    {
        var location = await CreateLocationAsync();

        var response = await _client.GetAsync($"/api/locations/{location.Id}/products?page=1&pageSize=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
