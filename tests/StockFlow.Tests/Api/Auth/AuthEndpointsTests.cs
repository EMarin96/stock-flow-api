using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using StockFlow.Api.Endpoints;
using StockFlow.Application.Auth.Login;
using StockFlow.Application.Users.Shared;
using StockFlow.Domain.Products;
using StockFlow.Domain.Users;
using StockFlow.Tests.Api;

namespace StockFlow.Tests.Api.Auth;

[Collection("Postgres collection")]
public class AuthEndpointsTests : IClassFixture<ApiFactoryFixture>, IAsyncLifetime
{
    // Mirrors the server's JsonStringEnumConverter registration (Program.cs), so
    // the test client can round-trip the Role enum as "Admin"/"Operator"/"ReadOnly" too.
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly ApiFactory _factory;
    private readonly HttpClient _anonymousClient;
    private readonly HttpClient _adminClient;

    public AuthEndpointsTests(ApiFactoryFixture factoryFixture)
    {
        _factory = factoryFixture.Factory;
        _anonymousClient = _factory.CreateClient();
        _adminClient = _factory.CreateAuthorizedClient(Role.Admin);
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<Guid> CreateUserAsync(string username, string password, Role role)
    {
        var response = await _adminClient.PostAsJsonAsync("/api/users", new CreateUserRequest(username, password, role), JsonOptions);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
        return created!.Id;
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokenAndExpiry()
    {
        await CreateUserAsync("alice", "correct-password-1", Role.Operator);

        var response = await _anonymousClient.PostAsJsonAsync("/api/auth/login", new LoginRequest("alice", "correct-password-1"), JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<LoginResponseDto>(JsonOptions);
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body!.Token));
        Assert.True(body.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_WithUnknownUsername_ReturnsUnauthorized()
    {
        var response = await _anonymousClient.PostAsJsonAsync("/api/auth/login", new LoginRequest("nobody", "whatever"), JsonOptions);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        await CreateUserAsync("alice", "correct-password-1", Role.Operator);

        var response = await _anonymousClient.PostAsJsonAsync("/api/auth/login", new LoginRequest("alice", "wrong-password"), JsonOptions);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ForDeactivatedUser_ReturnsUnauthorized()
    {
        var userId = await CreateUserAsync("alice", "correct-password-1", Role.Operator);
        await _adminClient.DeleteAsync($"/api/users/{userId}");

        var response = await _anonymousClient.PostAsJsonAsync("/api/auth/login", new LoginRequest("alice", "correct-password-1"), JsonOptions);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ThenUseToken_GrantsAccessAccordingToRole()
    {
        await CreateUserAsync("reader", "correct-password-1", Role.ReadOnly);
        var loginResponse = await _anonymousClient.PostAsJsonAsync(
            "/api/auth/login", new LoginRequest("reader", "correct-password-1"), JsonOptions);
        var body = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>(JsonOptions);

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.Token);

        var getResponse = await client.GetAsync("/api/products");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var postResponse = await client.PostAsJsonAsync(
            "/api/products",
            new CreateProductRequest("SKU-1", "Widget", null, "unit", 10m, Currency.USD, 5),
            JsonOptions);
        Assert.Equal(HttpStatusCode.Forbidden, postResponse.StatusCode);
    }

    [Fact]
    public async Task Login_WithMissingUsername_ReturnsBadRequest()
    {
        var response = await _anonymousClient.PostAsJsonAsync("/api/auth/login", new LoginRequest(string.Empty, "whatever"), JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
