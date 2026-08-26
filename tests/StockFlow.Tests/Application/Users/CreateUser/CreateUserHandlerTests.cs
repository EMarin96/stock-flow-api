using StockFlow.Application.Users.CreateUser;
using StockFlow.Domain.Users;
using StockFlow.Infrastructure.Security;

namespace StockFlow.Tests.Application.Users.CreateUser;

public class CreateUserHandlerTests
{
    private static CreateUserCommand ValidCommand(string username = "alice") => new(username, "password123", Role.Operator);

    [Fact]
    public async Task CreateUser_WhenCommandIsValidAndUsernameIsUnique_CreatesUser()
    {
        var repository = new InMemoryUserWriteRepository();
        var actingUserId = Guid.NewGuid();
        var handler = new CreateUserHandler(
            repository,
            new PasswordHasher(),
            new InMemoryCurrentUserService(actingUserId),
            new InMemoryUnitOfWork(),
            new CreateUserValidator());

        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("alice", result.Value.Username);
        Assert.Equal(Role.Operator, result.Value.Role);
        Assert.Equal(actingUserId, result.Value.CreatedBy);
    }

    [Fact]
    public async Task CreateUser_HashesThePassword_NeverStoresItInPlainText()
    {
        var repository = new InMemoryUserWriteRepository();
        var passwordHasher = new PasswordHasher();
        var handler = new CreateUserHandler(
            repository,
            passwordHasher,
            new InMemoryCurrentUserService(),
            new InMemoryUnitOfWork(),
            new CreateUserValidator());

        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        var storedUser = await repository.GetByIdAsync(result.Value.Id, CancellationToken.None);
        Assert.NotNull(storedUser);
        Assert.NotEqual("password123", storedUser.PasswordHash);
        Assert.True(passwordHasher.Verify("password123", storedUser.PasswordHash));
    }

    [Fact]
    public async Task CreateUser_WhenUsernameAlreadyExists_ReturnsConflict()
    {
        var existingUser = User.Create("alice", "some-hash", Role.ReadOnly, null);
        var repository = new InMemoryUserWriteRepository([existingUser]);
        var handler = new CreateUserHandler(
            repository,
            new PasswordHasher(),
            new InMemoryCurrentUserService(),
            new InMemoryUnitOfWork(),
            new CreateUserValidator());

        var result = await handler.Handle(ValidCommand("alice"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public async Task CreateUser_WhenUsernameDiffersOnlyByCase_ReturnsConflict()
    {
        var existingUser = User.Create("Alice", "some-hash", Role.ReadOnly, null);
        var repository = new InMemoryUserWriteRepository([existingUser]);
        var handler = new CreateUserHandler(
            repository,
            new PasswordHasher(),
            new InMemoryCurrentUserService(),
            new InMemoryUnitOfWork(),
            new CreateUserValidator());

        var result = await handler.Handle(ValidCommand("ALICE"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public async Task CreateUser_WhenCommandIsInvalid_ReturnsValidationError()
    {
        var repository = new InMemoryUserWriteRepository();
        var handler = new CreateUserHandler(
            repository,
            new PasswordHasher(),
            new InMemoryCurrentUserService(),
            new InMemoryUnitOfWork(),
            new CreateUserValidator());

        var result = await handler.Handle(ValidCommand() with { Password = "short" }, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }
}
