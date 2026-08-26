using StockFlow.Application.Users.GetUsers;
using StockFlow.Application.Users.Shared;
using StockFlow.Domain.Users;

namespace StockFlow.Tests.Application.Users.GetUsers;

public class GetUsersHandlerTests
{
    private static AuthenticationRecord SampleRecord(string username, Role role) =>
        new(Guid.NewGuid(), username, "hash", role, IsDeleted: false);

    [Fact]
    public async Task GetUsers_WhenCalled_ReturnsPagedUsers()
    {
        var repository = new InMemoryUserReadRepository(
            [SampleRecord("alice", Role.Admin), SampleRecord("bob", Role.Operator), SampleRecord("carol", Role.ReadOnly)]);
        var handler = new GetUsersHandler(repository, new GetUsersValidator());

        var result = await handler.Handle(new GetUsersQuery(1, 2, null, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.Equal(3, result.Value.TotalCount);
    }

    [Fact]
    public async Task GetUsers_FilteredByUsername_ReturnsOnlyMatchingUsers()
    {
        var repository = new InMemoryUserReadRepository([SampleRecord("alice", Role.Admin), SampleRecord("bob", Role.Operator)]);
        var handler = new GetUsersHandler(repository, new GetUsersValidator());

        var result = await handler.Handle(new GetUsersQuery(1, 20, "ali", null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal("alice", result.Value.Items[0].Username);
    }

    [Fact]
    public async Task GetUsers_FilteredByRole_ReturnsOnlyThatRole()
    {
        var repository = new InMemoryUserReadRepository([SampleRecord("alice", Role.Admin), SampleRecord("bob", Role.Operator)]);
        var handler = new GetUsersHandler(repository, new GetUsersValidator());

        var result = await handler.Handle(new GetUsersQuery(1, 20, null, Role.Operator), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal("bob", result.Value.Items[0].Username);
    }

    [Fact]
    public async Task GetUsers_WithInvalidPage_ReturnsValidationError()
    {
        var repository = new InMemoryUserReadRepository([]);
        var handler = new GetUsersHandler(repository, new GetUsersValidator());

        var result = await handler.Handle(new GetUsersQuery(0, 20, null, null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }
}
