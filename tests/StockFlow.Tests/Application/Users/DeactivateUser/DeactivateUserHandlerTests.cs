using StockFlow.Application.Users.DeactivateUser;
using StockFlow.Domain.Users;

namespace StockFlow.Tests.Application.Users.DeactivateUser;

public class DeactivateUserHandlerTests
{
    [Fact]
    public async Task DeactivateUser_WhenItExists_SoftDeletesUser()
    {
        var user = User.Create("alice", "hash", Role.Operator, null);
        var repository = new InMemoryUserWriteRepository([user]);
        var handler = new DeactivateUserHandler(repository, new InMemoryCurrentUserService(Guid.NewGuid()), new InMemoryUnitOfWork());

        var result = await handler.Handle(new DeactivateUserCommand(user.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(user.IsDeleted);
        Assert.NotNull(user.DeletedAt);
    }

    [Fact]
    public async Task DeactivateUser_WhenUserDoesNotExist_ReturnsNotFound()
    {
        var repository = new InMemoryUserWriteRepository();
        var handler = new DeactivateUserHandler(repository, new InMemoryCurrentUserService(), new InMemoryUnitOfWork());

        var result = await handler.Handle(new DeactivateUserCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task DeactivateUser_WhenTargetIsTheCurrentUser_ReturnsValidationError()
    {
        var user = User.Create("alice", "hash", Role.Admin, null);
        var repository = new InMemoryUserWriteRepository([user]);
        var handler = new DeactivateUserHandler(repository, new InMemoryCurrentUserService(user.Id), new InMemoryUnitOfWork());

        var result = await handler.Handle(new DeactivateUserCommand(user.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal("Users.CannotDeactivateSelf", result.Error.Code);
        Assert.False(user.IsDeleted);
    }
}
