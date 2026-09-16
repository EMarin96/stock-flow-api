using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using StockFlow.Api.Endpoints;
using StockFlow.Application.Common.Pagination;
using StockFlow.Application.Locations.Shared;
using StockFlow.Application.Products.Shared;
using StockFlow.Application.Reporting.Shared;
using StockFlow.Application.StockMovements.Shared;
using StockFlow.Domain.Locations;
using StockFlow.Domain.Products;
using StockFlow.Domain.StockMovements;
using StockFlow.Domain.Users;
using StockFlow.Tests.Api;

namespace StockFlow.Tests.Api.Reporting;

[Collection("Postgres collection")]
public class ReportingEndpointsTests : IClassFixture<ApiFactoryFixture>, IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public ReportingEndpointsTests(ApiFactoryFixture factoryFixture)
    {
        _factory = factoryFixture.Factory;
        _client = _factory.CreateAuthorizedClient(Role.Admin);
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<ProductDto> CreateProductAsync(string sku, int minimumStockThreshold = 5)
    {
        var request = new CreateProductRequest(sku, "Widget", null, "unit", 10m, Currency.USD, minimumStockThreshold);
        var response = await _client.PostAsJsonAsync("/api/products", request, JsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ProductDto>(JsonOptions))!;
    }

    private async Task<LocationDto> CreateLocationAsync(string code)
    {
        var request = new CreateLocationRequest(code, "Main Warehouse", null, null, null, "CA", "Los Angeles", Country.US);
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
    public async Task GetStockOverview_SumsQuantityAcrossLocationsAndFlagsLowStock()
    {
        var lowStockProduct = await CreateProductAsync("SKU-LOW", minimumStockThreshold: 10);
        var healthyProduct = await CreateProductAsync("SKU-HEALTHY", minimumStockThreshold: 10);
        var locationA = await CreateLocationAsync("WH-A");
        var locationB = await CreateLocationAsync("WH-B");

        await CreateMovementAsync(new CreateStockMovementRequest(lowStockProduct.Id, MovementType.In, 5, null, locationA.Id, null));
        await CreateMovementAsync(new CreateStockMovementRequest(healthyProduct.Id, MovementType.In, 20, null, locationA.Id, null));
        await CreateMovementAsync(new CreateStockMovementRequest(healthyProduct.Id, MovementType.In, 5, null, locationB.Id, null));

        var response = await _client.GetAsync("/api/reports/stock-overview?page=1&pageSize=50");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var page = await response.Content.ReadFromJsonAsync<PagedResult<StockOverviewDto>>(JsonOptions);
        var lowRow = page!.Items.Single(item => item.ProductId == lowStockProduct.Id);
        var healthyRow = page.Items.Single(item => item.ProductId == healthyProduct.Id);

        Assert.Equal(5, lowRow.TotalQuantity);
        Assert.True(lowRow.IsLowStock);
        Assert.Equal(25, healthyRow.TotalQuantity);
        Assert.False(healthyRow.IsLowStock);
    }

    [Fact]
    public async Task GetStockOverview_ForAProductWithNoStockAnywhere_ReturnsZeroNotOmitted()
    {
        var product = await CreateProductAsync("SKU-NOSTOCK", minimumStockThreshold: 5);

        var response = await _client.GetAsync("/api/reports/stock-overview?page=1&pageSize=50");
        var page = await response.Content.ReadFromJsonAsync<PagedResult<StockOverviewDto>>(JsonOptions);

        var row = page!.Items.Single(item => item.ProductId == product.Id);
        Assert.Equal(0, row.TotalQuantity);
        Assert.True(row.IsLowStock);
    }

    [Fact]
    public async Task GetStockOverview_WithLowStockOnly_ReturnsOnlyFlaggedProducts()
    {
        var lowStockProduct = await CreateProductAsync("SKU-LOW-2", minimumStockThreshold: 10);
        var healthyProduct = await CreateProductAsync("SKU-HEALTHY-2", minimumStockThreshold: 10);
        var location = await CreateLocationAsync("WH-C");
        await CreateMovementAsync(new CreateStockMovementRequest(healthyProduct.Id, MovementType.In, 50, null, location.Id, null));

        var response = await _client.GetAsync("/api/reports/stock-overview?page=1&pageSize=50&lowStockOnly=true");
        var page = await response.Content.ReadFromJsonAsync<PagedResult<StockOverviewDto>>(JsonOptions);

        Assert.DoesNotContain(page!.Items, item => item.ProductId == healthyProduct.Id);
        Assert.Contains(page.Items, item => item.ProductId == lowStockProduct.Id);
    }

    [Fact]
    public async Task GetStockOverview_WithInvalidPageSize_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/api/reports/stock-overview?page=1&pageSize=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetMovementActivity_GroupsByProductAndType()
    {
        var product = await CreateProductAsync("SKU-ACT-1");
        var location = await CreateLocationAsync("WH-D");

        await CreateMovementAsync(new CreateStockMovementRequest(product.Id, MovementType.In, 10, null, location.Id, null));
        await CreateMovementAsync(new CreateStockMovementRequest(product.Id, MovementType.In, 5, null, location.Id, null));
        await CreateMovementAsync(new CreateStockMovementRequest(product.Id, MovementType.Out, 3, location.Id, null, null));

        var response = await _client.GetAsync($"/api/reports/movement-activity?productId={product.Id}&page=1&pageSize=50");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var page = await response.Content.ReadFromJsonAsync<PagedResult<MovementActivityDto>>(JsonOptions);
        var inGroup = page!.Items.Single(item => item.Type == MovementType.In);
        var outGroup = page.Items.Single(item => item.Type == MovementType.Out);

        Assert.Equal(2, inGroup.MovementCount);
        Assert.Equal(15, inGroup.TotalQuantity);
        Assert.Equal(1, outGroup.MovementCount);
        Assert.Equal(3, outGroup.TotalQuantity);
    }

    [Fact]
    public async Task GetMovementActivity_FilteredByLocationId_MatchesEitherSourceOrDestination()
    {
        var product = await CreateProductAsync("SKU-ACT-2");
        var locationA = await CreateLocationAsync("WH-E");
        var locationB = await CreateLocationAsync("WH-F");

        await CreateMovementAsync(new CreateStockMovementRequest(product.Id, MovementType.In, 10, null, locationA.Id, null));
        await CreateMovementAsync(new CreateStockMovementRequest(product.Id, MovementType.Transfer, 4, locationA.Id, locationB.Id, null));

        var response = await _client.GetAsync($"/api/reports/movement-activity?locationId={locationA.Id}&page=1&pageSize=50");
        var page = await response.Content.ReadFromJsonAsync<PagedResult<MovementActivityDto>>(JsonOptions);

        Assert.Equal(2, page!.Items.Sum(item => item.MovementCount));
    }

    [Fact]
    public async Task GetMovementActivity_WithFromAfterTo_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/api/reports/movement-activity?from=2026-06-01&to=2026-01-01");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Reports_WithNoToken_ReturnUnauthorized()
    {
        var anonymousClient = _factory.CreateClient();

        var response = await anonymousClient.GetAsync("/api/reports/stock-overview");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Reports_WithAReadOnlyToken_Succeed()
    {
        var readOnlyClient = _factory.CreateAuthorizedClient(Role.ReadOnly);

        var overviewResponse = await readOnlyClient.GetAsync("/api/reports/stock-overview");
        var activityResponse = await readOnlyClient.GetAsync("/api/reports/movement-activity");

        Assert.Equal(HttpStatusCode.OK, overviewResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, activityResponse.StatusCode);
    }
}
