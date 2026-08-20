using StockFlow.Application.Locations.GetLocations;
using StockFlow.Application.Locations.Shared;
using StockFlow.Domain.Locations;

namespace StockFlow.Tests.Application.Locations.GetLocations;

public class GetLocationsHandlerTests
{
    private static LocationDto SampleDto(string code) => new(
        Guid.NewGuid(), code, "Main Warehouse", null, null, null, "CA", "Los Angeles", Country.US, DateTime.UtcNow, null, null, null);

    [Fact]
    public async Task GetLocations_WhenCalled_ReturnsPagedLocations()
    {
        var repository = new InMemoryLocationReadRepository([SampleDto("WH-1"), SampleDto("WH-2"), SampleDto("WH-3")]);
        var handler = new GetLocationsHandler(repository, new GetLocationsValidator());

        var result = await handler.Handle(new GetLocationsQuery(1, 2), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.Equal(3, result.Value.TotalCount);
        Assert.Equal(2, result.Value.TotalPages);
    }

    [Fact]
    public async Task GetLocations_WithInvalidPage_ReturnsValidationError()
    {
        var repository = new InMemoryLocationReadRepository([]);
        var handler = new GetLocationsHandler(repository, new GetLocationsValidator());

        var result = await handler.Handle(new GetLocationsQuery(0, 20), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }
}
