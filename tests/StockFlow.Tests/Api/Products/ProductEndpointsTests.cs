using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using StockFlow.Api.Endpoints;
using StockFlow.Application.Common.Pagination;
using StockFlow.Application.Products.Shared;
using StockFlow.Domain.Products;
using StockFlow.Tests.Api;

namespace StockFlow.Tests.Api.Products;

[Collection("Postgres collection")]
public class ProductEndpointsTests : IClassFixture<ApiFactoryFixture>, IAsyncLifetime
{
    // Mirrors the server's JsonStringEnumConverter registration (Program.cs), so
    // the test client can round-trip the Currency enum as "USD"/"CRC" strings too.
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public ProductEndpointsTests(ApiFactoryFixture factoryFixture)
    {
        _factory = factoryFixture.Factory;
        _client = _factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private static CreateProductRequest ValidRequest(string sku = "SKU-100") =>
        new(sku, "Widget", "A widget", "unit", 9.99m, Currency.USD, 5);

    [Fact]
    public async Task Create_ThenGetById_ReturnsTheCreatedProduct()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/products", ValidRequest());
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<ProductDto>(JsonOptions);
        Assert.NotNull(created);

        var getResponse = await _client.GetAsync($"/api/products/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<ProductDto>(JsonOptions);
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal("SKU-100", fetched.Sku);
    }

    [Fact]
    public async Task Create_WithDuplicateSku_ReturnsConflict()
    {
        var firstResponse = await _client.PostAsJsonAsync("/api/products", ValidRequest("SKU-DUP"));
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        var secondResponse = await _client.PostAsJsonAsync("/api/products", ValidRequest("SKU-DUP"));

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Create_WithNegativePrice_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/products",
            ValidRequest("SKU-NEG-PRICE") with { Price = -1m });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithNegativeMinimumStockThreshold_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/products",
            ValidRequest("SKU-NEG-THRESHOLD") with { MinimumStockThreshold = -1 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ForUnknownId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/products/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_ChangesEditableFields_ButNotTheSku()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/products", ValidRequest("SKU-UPDATE"));
        var created = await createResponse.Content.ReadFromJsonAsync<ProductDto>(JsonOptions);

        var updateRequest = new UpdateProductRequest("Widget v2", "Updated", "box", 15m, Currency.USD, 8);
        var updateResponse = await _client.PutAsJsonAsync($"/api/products/{created!.Id}", updateRequest);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<ProductDto>(JsonOptions);
        Assert.Equal("Widget v2", updated!.Name);
        Assert.Equal("SKU-UPDATE", updated.Sku);
    }

    [Fact]
    public async Task Update_ForUnknownId_ReturnsNotFound()
    {
        var updateRequest = new UpdateProductRequest("Widget", null, "unit", 10m, Currency.USD, 5);

        var response = await _client.PutAsJsonAsync($"/api/products/{Guid.NewGuid()}", updateRequest);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_SoftDeletes_SubsequentGetAndDeleteReturnNotFound()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/products", ValidRequest("SKU-DELETE"));
        var created = await createResponse.Content.ReadFromJsonAsync<ProductDto>(JsonOptions);

        var deleteResponse = await _client.DeleteAsync($"/api/products/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/products/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

        var secondDeleteResponse = await _client.DeleteAsync($"/api/products/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, secondDeleteResponse.StatusCode);
    }

    [Fact]
    public async Task List_WithPagination_ExcludesSoftDeletedProducts()
    {
        var toDelete = await _client.PostAsJsonAsync("/api/products", ValidRequest("SKU-LIST-DELETED"));
        var deletedProduct = await toDelete.Content.ReadFromJsonAsync<ProductDto>(JsonOptions);
        await _client.DeleteAsync($"/api/products/{deletedProduct!.Id}");

        for (var i = 0; i < 3; i++)
        {
            await _client.PostAsJsonAsync("/api/products", ValidRequest($"SKU-LIST-{i}"));
        }

        var response = await _client.GetAsync("/api/products?page=1&pageSize=2");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var page = await response.Content.ReadFromJsonAsync<PagedResult<ProductDto>>(JsonOptions);
        Assert.NotNull(page);
        Assert.Equal(2, page!.Items.Count);
        Assert.Equal(3, page.TotalCount); // the soft-deleted product must not be counted
        Assert.DoesNotContain(page.Items, item => item.Id == deletedProduct.Id);
    }

    [Fact]
    public async Task List_WithInvalidPageSize_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/api/products?page=1&pageSize=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
