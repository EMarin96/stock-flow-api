using StockFlow.Application.Users.GetUserById;
using StockFlow.Application.Users.Shared;
using StockFlow.Domain.Users;

namespace StockFlow.Tests.Application.Users.GetUserById;

public class GetUserByIdHandlerTests
{
    [Fact]
    public async Task GetUserById_WhenUserExists_ReturnsUser()
    {
        var record = new AuthenticationRecord(Guid.NewGuid(), "alice", "hash", Role.Admin, IsDeleted: false);
        var repository = new InMemoryUserReadRepository([record]);
        var handler = new GetUserByIdHandler(repository);

        var result = await handler.Handle(new GetUserByIdQuery(record.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("alice", result.Value.Username);
    }

    [Fact]
    public async Task GetUserById_ForUnknownId_ReturnsNotFound()
    {
        var repository = new InMemoryUserReadRepository([]);
        var handler = new GetUserByIdHandler(repository);

        var result = await handler.Handle(new GetUserByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task GetUserById_ForDeactivatedUser_ReturnsNotFound()
    {
        var record = new AuthenticationRecord(Guid.NewGuid(), "alice", "hash", Role.Admin, IsDeleted: true);
        var repository = new InMemoryUserReadRepository([record]);
        var handler = new GetUserByIdHandler(repository);

        var result = await handler.Handle(new GetUserByIdQuery(record.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}
