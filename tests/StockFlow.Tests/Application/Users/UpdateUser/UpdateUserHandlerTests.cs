using StockFlow.Application.Users.UpdateUser;
using StockFlow.Domain.Users;
using StockFlow.Infrastructure.Security;

namespace StockFlow.Tests.Application.Users.UpdateUser;

public class UpdateUserHandlerTests
{
    [Fact]
    public async Task UpdateUser_WhenItExists_UpdatesRole()
    {
        var user = User.Create("alice", "old-hash", Role.ReadOnly, null);
        var repository = new InMemoryUserWriteRepository([user]);
        var handler = new UpdateUserHandler(
            repository,
            new PasswordHasher(),
            new InMemoryCurrentUserService(),
            new InMemoryUnitOfWork(),
            new UpdateUserValidator());

        var result = await handler.Handle(new UpdateUserCommand(user.Id, Role.Admin, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(Role.Admin, result.Value.Role);
        Assert.Equal("old-hash", user.PasswordHash); // unchanged — no NewPassword supplied
    }

    [Fact]
    public async Task UpdateUser_WithNewPassword_ResetsPassword()
    {
        var user = User.Create("alice", "old-hash", Role.ReadOnly, null);
        var repository = new InMemoryUserWriteRepository([user]);
        var passwordHasher = new PasswordHasher();
        var handler = new UpdateUserHandler(
            repository,
            passwordHasher,
            new InMemoryCurrentUserService(),
            new InMemoryUnitOfWork(),
            new UpdateUserValidator());

        var result = await handler.Handle(new UpdateUserCommand(user.Id, Role.ReadOnly, "brand-new-password"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual("old-hash", user.PasswordHash);
        Assert.True(passwordHasher.Verify("brand-new-password", user.PasswordHash));
    }

    [Fact]
    public async Task UpdateUser_WhenUserDoesNotExist_ReturnsNotFound()
    {
        var repository = new InMemoryUserWriteRepository();
        var handler = new UpdateUserHandler(
            repository,
            new PasswordHasher(),
            new InMemoryCurrentUserService(),
            new InMemoryUnitOfWork(),
            new UpdateUserValidator());

        var result = await handler.Handle(new UpdateUserCommand(Guid.NewGuid(), Role.Admin, null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task UpdateUser_WhenCommandIsInvalid_ReturnsValidationError()
    {
        var repository = new InMemoryUserWriteRepository();
        var handler = new UpdateUserHandler(
            repository,
            new PasswordHasher(),
            new InMemoryCurrentUserService(),
            new InMemoryUnitOfWork(),
            new UpdateUserValidator());

        var result = await handler.Handle(new UpdateUserCommand(Guid.NewGuid(), Role.Admin, "short"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }
}
