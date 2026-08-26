using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using StockFlow.Api.Endpoints;
using StockFlow.Domain.Locations;
using StockFlow.Domain.Products;
using StockFlow.Domain.Users;
using StockFlow.Tests.Api;

namespace StockFlow.Tests.Api.Authorization;

/// <summary>
/// Explicit 401 (no/invalid token) and 403 (wrong role) coverage per endpoint
/// group, on top of the happy-path coverage already in
/// ProductEndpointsTests/LocationEndpointsTests/StockMovementEndpointsTests/
/// UserEndpointsTests (see tasks.md).
/// </summary>
[Collection("Postgres collection")]
public class AuthorizationTests : IClassFixture<ApiFactoryFixture>, IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly ApiFactory _factory;

    public AuthorizationTests(ApiFactoryFixture factoryFixture)
    {
        _factory = factoryFixture.Factory;
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private static CreateProductRequest ValidProductRequest(string sku = "SKU-AUTH-100") =>
        new(sku, "Widget", null, "unit", 9.99m, Currency.USD, 5);

    private static CreateLocationRequest ValidLocationRequest(string code = "WH-AUTH-100") =>
        new(code, "Main Warehouse", null, null, null, "CA", "Los Angeles", Country.US);

    [Theory]
    [InlineData("GET", "/api/products")]
    [InlineData("POST", "/api/products")]
    [InlineData("GET", "/api/locations")]
    [InlineData("POST", "/api/locations")]
    [InlineData("GET", "/api/stock-movements")]
    [InlineData("POST", "/api/stock-movements")]
    [InlineData("GET", "/api/users")]
    [InlineData("POST", "/api/users")]
    public async Task Request_WithNoToken_ReturnsUnauthorized(string method, string path)
    {
        var client = _factory.CreateClient();

        var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), path));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Request_WithInvalidToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "this-is-not-a-valid-jwt");

        var response = await client.GetAsync("/api/products");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Request_WithTamperedToken_ReturnsUnauthorized()
    {
        using var scope = _factory.Services.CreateScope();
        var jwtTokenGenerator = scope.ServiceProvider
            .GetRequiredService<StockFlow.Application.Common.Security.IJwtTokenGenerator>();

        // A tampered signature fails validation the same way an expired token
        // does — both surface as 401 Unauthorized (see spec.md — acceptance
        // criteria: "no token or an invalid/expired token").
        var (token, _) = jwtTokenGenerator.GenerateToken(Guid.NewGuid(), "someone", Role.Admin);
        var tamperedToken = token[..^2] + (token[^2] == 'a' ? "bb" : "aa");

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tamperedToken);

        var response = await client.GetAsync("/api/products");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData(Role.ReadOnly)]
    [InlineData(Role.Operator)]
    [InlineData(Role.Admin)]
    public async Task Get_WithAnyRole_ReturnsOk(Role role)
    {
        var client = _factory.CreateAuthorizedClient(role);

        var response = await client.GetAsync("/api/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_AsReadOnly_ReturnsForbidden()
    {
        var client = _factory.CreateAuthorizedClient(Role.ReadOnly);

        var response = await client.PostAsJsonAsync("/api/products", ValidProductRequest(), JsonOptions);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_AsOperator_ReturnsCreated()
    {
        var client = _factory.CreateAuthorizedClient(Role.Operator);

        var response = await client.PostAsJsonAsync("/api/products", ValidProductRequest(), JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_AsOperator_ReturnsForbidden()
    {
        var adminClient = _factory.CreateAuthorizedClient(Role.Admin);
        var created = await (await adminClient.PostAsJsonAsync("/api/products", ValidProductRequest(), JsonOptions))
            .Content.ReadFromJsonAsync<StockFlow.Application.Products.Shared.ProductDto>(JsonOptions);

        var operatorClient = _factory.CreateAuthorizedClient(Role.Operator);
        var response = await operatorClient.DeleteAsync($"/api/products/{created!.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_AsAdmin_ReturnsNoContent()
    {
        var adminClient = _factory.CreateAuthorizedClient(Role.Admin);
        var created = await (await adminClient.PostAsJsonAsync("/api/products", ValidProductRequest(), JsonOptions))
            .Content.ReadFromJsonAsync<StockFlow.Application.Products.Shared.ProductDto>(JsonOptions);

        var response = await adminClient.DeleteAsync($"/api/products/{created!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteLocation_AsOperator_ReturnsForbidden()
    {
        var adminClient = _factory.CreateAuthorizedClient(Role.Admin);
        var created = await (await adminClient.PostAsJsonAsync("/api/locations", ValidLocationRequest(), JsonOptions))
            .Content.ReadFromJsonAsync<StockFlow.Application.Locations.Shared.LocationDto>(JsonOptions);

        var operatorClient = _factory.CreateAuthorizedClient(Role.Operator);
        var response = await operatorClient.DeleteAsync($"/api/locations/{created!.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateLocation_AsReadOnly_ReturnsForbidden()
    {
        var client = _factory.CreateAuthorizedClient(Role.ReadOnly);

        var response = await client.PostAsJsonAsync("/api/locations", ValidLocationRequest(), JsonOptions);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData(Role.ReadOnly)]
    [InlineData(Role.Operator)]
    public async Task UsersEndpoints_AsNonAdmin_ReturnForbidden(Role role)
    {
        var client = _factory.CreateAuthorizedClient(role);

        var getResponse = await client.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.Forbidden, getResponse.StatusCode);

        var createResponse = await client.PostAsJsonAsync(
            "/api/users", new CreateUserRequest("someone", "correct-password-1", Role.ReadOnly), JsonOptions);
        Assert.Equal(HttpStatusCode.Forbidden, createResponse.StatusCode);
    }

    [Fact]
    public async Task UsersEndpoints_AsAdmin_ReturnsOk()
    {
        var client = _factory.CreateAuthorizedClient(Role.Admin);

        var response = await client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
