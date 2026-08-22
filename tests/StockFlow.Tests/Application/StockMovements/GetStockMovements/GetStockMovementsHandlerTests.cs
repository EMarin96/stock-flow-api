using StockFlow.Application.StockMovements.GetStockMovements;
using StockFlow.Application.StockMovements.Shared;
using StockFlow.Domain.StockMovements;

namespace StockFlow.Tests.Application.StockMovements.GetStockMovements;

public class GetStockMovementsHandlerTests
{
    private static StockMovementDto SampleDto(
        Guid productId, MovementType type, Guid? sourceLocationId = null, Guid? destinationLocationId = null) => new(
        Guid.NewGuid(), productId, type, 10, sourceLocationId, destinationLocationId, null, DateTime.UtcNow, null);

    [Fact]
    public async Task GetStockMovements_WhenCalled_ReturnsPagedMovements()
    {
        var productId = Guid.NewGuid();
        var repository = new InMemoryStockMovementReadRepository(
            [SampleDto(productId, MovementType.In), SampleDto(productId, MovementType.In), SampleDto(productId, MovementType.In)]);
        var handler = new GetStockMovementsHandler(repository, new GetStockMovementsValidator());

        var result = await handler.Handle(new GetStockMovementsQuery(1, 2, null, null, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.Equal(3, result.Value.TotalCount);
    }

    [Fact]
    public async Task GetStockMovements_FilteredByProductId_ReturnsOnlyMatchingMovements()
    {
        var wantedProductId = Guid.NewGuid();
        var otherProductId = Guid.NewGuid();
        var repository = new InMemoryStockMovementReadRepository(
            [SampleDto(wantedProductId, MovementType.In), SampleDto(otherProductId, MovementType.In)]);
        var handler = new GetStockMovementsHandler(repository, new GetStockMovementsValidator());

        var result = await handler.Handle(new GetStockMovementsQuery(1, 20, wantedProductId, null, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal(wantedProductId, result.Value.Items[0].ProductId);
    }

    [Fact]
    public async Task GetStockMovements_FilteredByLocationId_MatchesEitherSourceOrDestination()
    {
        var productId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var otherLocationId = Guid.NewGuid();
        var repository = new InMemoryStockMovementReadRepository(
        [
            SampleDto(productId, MovementType.In, destinationLocationId: locationId),
            SampleDto(productId, MovementType.Out, sourceLocationId: locationId),
            SampleDto(productId, MovementType.Out, sourceLocationId: otherLocationId),
        ]);
        var handler = new GetStockMovementsHandler(repository, new GetStockMovementsValidator());

        var result = await handler.Handle(new GetStockMovementsQuery(1, 20, null, locationId, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Items.Count);
    }

    [Fact]
    public async Task GetStockMovements_FilteredByType_ReturnsOnlyMatchingMovements()
    {
        var productId = Guid.NewGuid();
        var repository = new InMemoryStockMovementReadRepository(
            [SampleDto(productId, MovementType.In), SampleDto(productId, MovementType.Out)]);
        var handler = new GetStockMovementsHandler(repository, new GetStockMovementsValidator());

        var result = await handler.Handle(new GetStockMovementsQuery(1, 20, null, null, MovementType.Out), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal(MovementType.Out, result.Value.Items[0].Type);
    }

    [Fact]
    public async Task GetStockMovements_WithInvalidPage_ReturnsValidationError()
    {
        var repository = new InMemoryStockMovementReadRepository([]);
        var handler = new GetStockMovementsHandler(repository, new GetStockMovementsValidator());

        var result = await handler.Handle(new GetStockMovementsQuery(0, 20, null, null, null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }
}
