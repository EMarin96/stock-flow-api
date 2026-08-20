using StockFlow.Application.Locations.GetLocationById;
using StockFlow.Application.Locations.Shared;
using StockFlow.Domain.Locations;

namespace StockFlow.Tests.Application.Locations.GetLocationById;

public class GetLocationByIdHandlerTests
{
    private static LocationDto SampleDto(Guid id) => new(
        id, "WH-100", "Main Warehouse", "123 Main St", null, null, "CA", "Los Angeles", Country.US, DateTime.UtcNow, null, null, null);

    [Fact]
    public async Task GetLocationById_WhenItExists_ReturnsLocation()
    {
        var id = Guid.NewGuid();
        var repository = new InMemoryLocationReadRepository([SampleDto(id)]);
        var handler = new GetLocationByIdHandler(repository);

        var result = await handler.Handle(new GetLocationByIdQuery(id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(id, result.Value.Id);
    }

    [Fact]
    public async Task GetLocationById_WhenLocationDoesNotExist_ReturnsNotFound()
    {
        var repository = new InMemoryLocationReadRepository([]);
        var handler = new GetLocationByIdHandler(repository);

        var result = await handler.Handle(new GetLocationByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task GetLocationById_WhenLocationWasSoftDeleted_ReturnsNotFound()
    {
        // The read repository (Dapper in production) is expected to never surface
        // soft-deleted locations in the first place — simulated here by an empty seed.
        var id = Guid.NewGuid();
        var repository = new InMemoryLocationReadRepository([]);
        var handler = new GetLocationByIdHandler(repository);

        var result = await handler.Handle(new GetLocationByIdQuery(id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}
