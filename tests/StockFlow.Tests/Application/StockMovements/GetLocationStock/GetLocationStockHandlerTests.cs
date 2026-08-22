using StockFlow.Application.Locations.Shared;
using StockFlow.Application.StockMovements.GetLocationStock;
using StockFlow.Application.StockMovements.Shared;
using StockFlow.Domain.Locations;

namespace StockFlow.Tests.Application.StockMovements.GetLocationStock;

public class GetLocationStockHandlerTests
{
    private static LocationDto SampleLocationDto(Guid id) => new(
        id, "WH-100", "Main Warehouse", null, null, null, "CA", "Los Angeles", Country.US, DateTime.UtcNow, null, null, null);

    [Fact]
    public async Task GetLocationStock_WhenLocationExists_ReturnsPagedStockIncludingZeroQuantity()
    {
        var locationId = Guid.NewGuid();
        var locationReadRepository = new InMemoryLocationReadRepository([SampleLocationDto(locationId)]);
        var stockLevelReadRepository = new InMemoryStockLevelReadRepository(
        [
            new LocationStockDto(Guid.NewGuid(), "SKU-1", "Widget", 10),
            new LocationStockDto(Guid.NewGuid(), "SKU-2", "Gadget", 0),
        ]);
        var handler = new GetLocationStockHandler(locationReadRepository, stockLevelReadRepository, new GetLocationStockValidator());

        var result = await handler.Handle(new GetLocationStockQuery(locationId, null, 1, 20), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.Contains(result.Value.Items, item => item.Quantity == 0);
    }

    [Fact]
    public async Task GetLocationStock_FilteredByProductName_ReturnsOnlyMatchingProducts()
    {
        var locationId = Guid.NewGuid();
        var locationReadRepository = new InMemoryLocationReadRepository([SampleLocationDto(locationId)]);
        var stockLevelReadRepository = new InMemoryStockLevelReadRepository(
        [
            new LocationStockDto(Guid.NewGuid(), "SKU-1", "Widget", 10),
            new LocationStockDto(Guid.NewGuid(), "SKU-2", "Gadget", 5),
        ]);
        var handler = new GetLocationStockHandler(locationReadRepository, stockLevelReadRepository, new GetLocationStockValidator());

        var result = await handler.Handle(new GetLocationStockQuery(locationId, "Widg", 1, 20), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal("Widget", result.Value.Items[0].ProductName);
    }

    [Fact]
    public async Task GetLocationStock_WhenLocationDoesNotExist_ReturnsNotFound()
    {
        var locationReadRepository = new InMemoryLocationReadRepository([]);
        var stockLevelReadRepository = new InMemoryStockLevelReadRepository([]);
        var handler = new GetLocationStockHandler(locationReadRepository, stockLevelReadRepository, new GetLocationStockValidator());

        var result = await handler.Handle(new GetLocationStockQuery(Guid.NewGuid(), null, 1, 20), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal("Locations.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task GetLocationStock_WithInvalidPage_ReturnsValidationError()
    {
        var locationReadRepository = new InMemoryLocationReadRepository([]);
        var stockLevelReadRepository = new InMemoryStockLevelReadRepository([]);
        var handler = new GetLocationStockHandler(locationReadRepository, stockLevelReadRepository, new GetLocationStockValidator());

        var result = await handler.Handle(new GetLocationStockQuery(Guid.NewGuid(), null, 0, 20), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }
}
