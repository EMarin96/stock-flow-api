using StockFlow.Application.Reporting.GetMovementActivity;
using StockFlow.Domain.StockMovements;

namespace StockFlow.Tests.Application.Reporting.GetMovementActivity;

public class GetMovementActivityHandlerTests
{
    private static InMemoryReportingReadRepository.MovementSeed SampleMovement(
        Guid productId,
        MovementType type,
        int quantity = 10,
        Guid? sourceLocationId = null,
        Guid? destinationLocationId = null,
        DateTime? createdAt = null) => new(
        productId, "SKU-1", "Widget", type, quantity, sourceLocationId, destinationLocationId, createdAt ?? DateTime.UtcNow);

    [Fact]
    public async Task GetMovementActivity_GroupsByProductAndType()
    {
        var productId = Guid.NewGuid();
        var repository = new InMemoryReportingReadRepository(movementSeed:
        [
            SampleMovement(productId, MovementType.In, quantity: 10),
            SampleMovement(productId, MovementType.In, quantity: 5),
            SampleMovement(productId, MovementType.Out, quantity: 3),
        ]);
        var handler = new GetMovementActivityHandler(repository, new GetMovementActivityValidator());

        var result = await handler.Handle(new GetMovementActivityQuery(null, null, null, null, 1, 20), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Items.Count);
        var inGroup = result.Value.Items.Single(item => item.Type == MovementType.In);
        Assert.Equal(2, inGroup.MovementCount);
        Assert.Equal(15, inGroup.TotalQuantity);
    }

    [Fact]
    public async Task GetMovementActivity_FilteredByDateRange_ExcludesMovementsOutsideRange()
    {
        var productId = Guid.NewGuid();
        var repository = new InMemoryReportingReadRepository(movementSeed:
        [
            SampleMovement(productId, MovementType.In, createdAt: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
            SampleMovement(productId, MovementType.In, createdAt: new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc)),
        ]);
        var handler = new GetMovementActivityHandler(repository, new GetMovementActivityValidator());

        var query = new GetMovementActivityQuery(
            new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc),
            null, null, 1, 20);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal(1, result.Value.Items[0].MovementCount);
    }

    [Fact]
    public async Task GetMovementActivity_FilteredByProductId_ReturnsOnlyMatchingProduct()
    {
        var wantedProductId = Guid.NewGuid();
        var otherProductId = Guid.NewGuid();
        var repository = new InMemoryReportingReadRepository(movementSeed:
        [
            SampleMovement(wantedProductId, MovementType.In),
            SampleMovement(otherProductId, MovementType.In),
        ]);
        var handler = new GetMovementActivityHandler(repository, new GetMovementActivityValidator());

        var result = await handler.Handle(new GetMovementActivityQuery(null, null, wantedProductId, null, 1, 20), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal(wantedProductId, result.Value.Items[0].ProductId);
    }

    [Fact]
    public async Task GetMovementActivity_FilteredByLocationId_MatchesEitherSourceOrDestination()
    {
        var productId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var otherLocationId = Guid.NewGuid();
        var repository = new InMemoryReportingReadRepository(movementSeed:
        [
            SampleMovement(productId, MovementType.In, destinationLocationId: locationId),
            SampleMovement(productId, MovementType.Out, sourceLocationId: locationId),
            SampleMovement(productId, MovementType.Out, sourceLocationId: otherLocationId),
        ]);
        var handler = new GetMovementActivityHandler(repository, new GetMovementActivityValidator());

        var result = await handler.Handle(new GetMovementActivityQuery(null, null, null, locationId, 1, 20), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Items.Count);
    }

    [Fact]
    public async Task GetMovementActivity_WhenFromIsAfterTo_ReturnsValidationError()
    {
        var repository = new InMemoryReportingReadRepository();
        var handler = new GetMovementActivityHandler(repository, new GetMovementActivityValidator());

        var query = new GetMovementActivityQuery(
            new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            null, null, 1, 20);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public async Task GetMovementActivity_WithInvalidPage_ReturnsValidationError()
    {
        var repository = new InMemoryReportingReadRepository();
        var handler = new GetMovementActivityHandler(repository, new GetMovementActivityValidator());

        var result = await handler.Handle(new GetMovementActivityQuery(null, null, null, null, 0, 20), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }
}
