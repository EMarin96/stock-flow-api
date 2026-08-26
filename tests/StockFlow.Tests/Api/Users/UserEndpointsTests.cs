using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using StockFlow.Api.Endpoints;
using StockFlow.Application.Auth.Login;
using StockFlow.Application.Common.Pagination;
using StockFlow.Application.Users.Shared;
using StockFlow.Domain.Users;
using StockFlow.Tests.Api;

namespace StockFlow.Tests.Api.Users;

[Collection("Postgres collection")]
public class UserEndpointsTests : IClassFixture<ApiFactoryFixture>, IAsyncLifetime
{
    // Mirrors the server's JsonStringEnumConverter registration (Program.cs), so
    // the test client can round-trip the Role enum as "Admin"/"Operator"/"ReadOnly" too.
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public UserEndpointsTests(ApiFactoryFixture factoryFixture)
    {
        _factory = factoryFixture.Factory;
        _client = _factory.CreateAuthorizedClient(Role.Admin);
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private static CreateUserRequest ValidRequest(string username = "alice", Role role = Role.Operator) =>
        new(username, "correct-password-1", role);

    [Fact]
    public async Task Create_ThenGetById_ReturnsTheCreatedUser_WithoutAnyPasswordField()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/users", ValidRequest(), JsonOptions);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdBody = await createResponse.Content.ReadAsStringAsync();
        Assert.DoesNotContain("password", createdBody, StringComparison.OrdinalIgnoreCase);

        var created = await createResponse.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
        Assert.NotNull(created);

        var getResponse = await _client.GetAsync($"/api/users/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var getBody = await getResponse.Content.ReadAsStringAsync();
        Assert.DoesNotContain("password", getBody, StringComparison.OrdinalIgnoreCase);

        var fetched = await getResponse.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
        Assert.Equal("alice", fetched!.Username);
        Assert.Equal(Role.Operator, fetched.Role);
    }

    [Fact]
    public async Task Create_WithDuplicateUsername_ReturnsConflict()
    {
        var firstResponse = await _client.PostAsJsonAsync("/api/users", ValidRequest("bob"), JsonOptions);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        var secondResponse = await _client.PostAsJsonAsync("/api/users", ValidRequest("bob"), JsonOptions);

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Create_WithUsernameDifferingOnlyByCase_ReturnsConflict()
    {
        await _client.PostAsJsonAsync("/api/users", ValidRequest("carol"), JsonOptions);

        var response = await _client.PostAsJsonAsync("/api/users", ValidRequest("CAROL"), JsonOptions);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithShortPassword_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/users",
            new CreateUserRequest("dave", "short", Role.ReadOnly),
            JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ForUnknownId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/users/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task List_WithPagination_ExcludesDeactivatedUsersByDefault()
    {
        var toDeactivate = await _client.PostAsJsonAsync("/api/users", ValidRequest("to-deactivate"), JsonOptions);
        var deactivatedUser = await toDeactivate.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
        await _client.DeleteAsync($"/api/users/{deactivatedUser!.Id}");

        for (var i = 0; i < 3; i++)
        {
            await _client.PostAsJsonAsync("/api/users", ValidRequest($"user-{i}"), JsonOptions);
        }

        var response = await _client.GetAsync("/api/users?page=1&pageSize=2");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var page = await response.Content.ReadFromJsonAsync<PagedResult<UserDto>>(JsonOptions);
        Assert.NotNull(page);
        Assert.Equal(2, page!.Items.Count);
        Assert.DoesNotContain(page.Items, item => item.Id == deactivatedUser.Id);
    }

    [Fact]
    public async Task List_FilteredByUsername_ReturnsOnlyMatchingUsers()
    {
        await _client.PostAsJsonAsync("/api/users", ValidRequest("filter-target"), JsonOptions);
        await _client.PostAsJsonAsync("/api/users", ValidRequest("someone-else"), JsonOptions);

        var response = await _client.GetAsync("/api/users?username=filter-tar");

        var page = await response.Content.ReadFromJsonAsync<PagedResult<UserDto>>(JsonOptions);
        Assert.Single(page!.Items);
        Assert.Equal("filter-target", page.Items[0].Username);
    }

    [Fact]
    public async Task List_FilteredByRole_ReturnsOnlyThatRole()
    {
        await _client.PostAsJsonAsync("/api/users", ValidRequest("op-user", Role.Operator), JsonOptions);
        await _client.PostAsJsonAsync("/api/users", ValidRequest("ro-user", Role.ReadOnly), JsonOptions);

        var response = await _client.GetAsync("/api/users?role=ReadOnly");

        var page = await response.Content.ReadFromJsonAsync<PagedResult<UserDto>>(JsonOptions);
        Assert.Single(page!.Items);
        Assert.Equal("ro-user", page.Items[0].Username);
    }

    [Fact]
    public async Task Update_ChangesRole_ButNotUsername()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/users", ValidRequest("eve", Role.ReadOnly), JsonOptions);
        var created = await createResponse.Content.ReadFromJsonAsync<UserDto>(JsonOptions);

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/users/{created!.Id}", new UpdateUserRequest(Role.Admin, null), JsonOptions);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
        Assert.Equal(Role.Admin, updated!.Role);
        Assert.Equal("eve", updated.Username);
    }

    [Fact]
    public async Task Update_WithNewPassword_AllowsLoginWithTheNewPassword()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/users", ValidRequest("frank"), JsonOptions);
        var created = await createResponse.Content.ReadFromJsonAsync<UserDto>(JsonOptions);

        await _client.PutAsJsonAsync(
            $"/api/users/{created!.Id}", new UpdateUserRequest(Role.Operator, "brand-new-password-1"), JsonOptions);

        using var anonymousClient = _factory.CreateClient();
        var loginResponse = await anonymousClient.PostAsJsonAsync(
            "/api/auth/login", new LoginRequest("frank", "brand-new-password-1"), JsonOptions);

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task Update_ForUnknownId_ReturnsNotFound()
    {
        var response = await _client.PutAsJsonAsync(
            $"/api/users/{Guid.NewGuid()}", new UpdateUserRequest(Role.Admin, null), JsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_DeactivatesUser_SubsequentGetReturnsNotFound()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/users", ValidRequest("grace"), JsonOptions);
        var created = await createResponse.Content.ReadFromJsonAsync<UserDto>(JsonOptions);

        var deleteResponse = await _client.DeleteAsync($"/api/users/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/users/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_ForUnknownId_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync($"/api/users/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_OwnAccount_ReturnsBadRequestAndLeavesAccountActive()
    {
        // A real, self-authenticated flow: create an admin, log in as that exact
        // admin (so the token's subject is that user's real id), then attempt to
        // deactivate that same id.
        var createResponse = await _client.PostAsJsonAsync("/api/users", ValidRequest("self-admin", Role.Admin), JsonOptions);
        var created = await createResponse.Content.ReadFromJsonAsync<UserDto>(JsonOptions);

        using var anonymousClient = _factory.CreateClient();
        var loginResponse = await anonymousClient.PostAsJsonAsync(
            "/api/auth/login", new LoginRequest("self-admin", "correct-password-1"), JsonOptions);
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>(JsonOptions);

        using var selfClient = _factory.CreateClient();
        selfClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginBody!.Token);

        var deleteResponse = await selfClient.DeleteAsync($"/api/users/{created!.Id}");

        Assert.Equal(HttpStatusCode.BadRequest, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/users/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }
}
