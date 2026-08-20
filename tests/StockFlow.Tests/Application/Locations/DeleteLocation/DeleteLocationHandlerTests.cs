using StockFlow.Application.Locations.DeleteLocation;
using StockFlow.Domain.Locations;

namespace StockFlow.Tests.Application.Locations.DeleteLocation;

public class DeleteLocationHandlerTests
{
    [Fact]
    public async Task DeleteLocation_WhenItExists_SoftDeletesLocation()
    {
        var location = Location.Create("WH-100", "Main Warehouse", null, null, null, "CA", "Los Angeles", Country.US);
        var repository = new InMemoryLocationWriteRepository([location]);
        var handler = new DeleteLocationHandler(repository, new InMemoryUnitOfWork());

        var result = await handler.Handle(new DeleteLocationCommand(location.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(location.IsDeleted);
        Assert.NotNull(location.DeletedAt);
    }

    [Fact]
    public async Task DeleteLocation_WhenLocationDoesNotExist_ReturnsNotFound()
    {
        var repository = new InMemoryLocationWriteRepository();
        var handler = new DeleteLocationHandler(repository, new InMemoryUnitOfWork());

        var result = await handler.Handle(new DeleteLocationCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task DeleteLocation_WhenLocationIsAlreadyDeleted_ReturnsNotFound()
    {
        var location = Location.Create("WH-100", "Main Warehouse", null, null, null, "CA", "Los Angeles", Country.US);
        location.SoftDelete();
        var repository = new InMemoryLocationWriteRepository([location]);
        var handler = new DeleteLocationHandler(repository, new InMemoryUnitOfWork());

        var result = await handler.Handle(new DeleteLocationCommand(location.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}
