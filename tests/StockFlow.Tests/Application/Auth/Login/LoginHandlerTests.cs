using Microsoft.Extensions.Options;
using StockFlow.Application.Auth.Login;
using StockFlow.Application.Users.Shared;
using StockFlow.Domain.Users;
using StockFlow.Infrastructure.Security;

namespace StockFlow.Tests.Application.Auth.Login;

public class LoginHandlerTests
{
    private static readonly PasswordHasher PasswordHasher = new();

    private static JwtTokenGenerator NewTokenGenerator() =>
        new(Options.Create(new JwtOptions
        {
            Secret = "unit-test-signing-key-not-for-production-use-only-32chars+",
            Issuer = "StockFlowApi",
            Audience = "StockFlowApi",
            ExpiryMinutes = 480,
        }));

    private static AuthenticationRecord ActiveUser(string username, string password, Role role) =>
        new(Guid.NewGuid(), username, PasswordHasher.Hash(password), role, IsDeleted: false);

    [Fact]
    public async Task Login_WithCorrectCredentials_ReturnsTokenAndExpiry()
    {
        var record = ActiveUser("alice", "correct-password", Role.Admin);
        var repository = new InMemoryUserReadRepository([record]);
        var handler = new LoginHandler(repository, PasswordHasher, NewTokenGenerator(), new LoginValidator());

        var result = await handler.Handle(new LoginCommand("alice", "correct-password"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(result.Value.Token));
        Assert.True(result.Value.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_WithUnknownUsername_ReturnsInvalidCredentials()
    {
        var repository = new InMemoryUserReadRepository([]);
        var handler = new LoginHandler(repository, PasswordHasher, NewTokenGenerator(), new LoginValidator());

        var result = await handler.Handle(new LoginCommand("unknown", "whatever"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.Error.Type);
        Assert.Equal("Auth.InvalidCredentials", result.Error.Code);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsInvalidCredentials()
    {
        var record = ActiveUser("alice", "correct-password", Role.Admin);
        var repository = new InMemoryUserReadRepository([record]);
        var handler = new LoginHandler(repository, PasswordHasher, NewTokenGenerator(), new LoginValidator());

        var result = await handler.Handle(new LoginCommand("alice", "wrong-password"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.Error.Type);
        Assert.Equal("Auth.InvalidCredentials", result.Error.Code);
    }

    [Fact]
    public async Task Login_ForDeactivatedUser_ReturnsInvalidCredentials()
    {
        var record = ActiveUser("alice", "correct-password", Role.Admin) with { IsDeleted = true };
        var repository = new InMemoryUserReadRepository([record]);
        var handler = new LoginHandler(repository, PasswordHasher, NewTokenGenerator(), new LoginValidator());

        var result = await handler.Handle(new LoginCommand("alice", "correct-password"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.Error.Type);
        Assert.Equal("Auth.InvalidCredentials", result.Error.Code);
    }

    [Fact]
    public async Task Login_WithEmptyUsername_ReturnsValidationError()
    {
        var repository = new InMemoryUserReadRepository([]);
        var handler = new LoginHandler(repository, PasswordHasher, NewTokenGenerator(), new LoginValidator());

        var result = await handler.Handle(new LoginCommand(string.Empty, "whatever"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }
}
