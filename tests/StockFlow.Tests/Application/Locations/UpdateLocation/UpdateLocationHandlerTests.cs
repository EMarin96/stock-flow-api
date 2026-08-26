using StockFlow.Application.Locations.Shared;
using StockFlow.Application.Locations.UpdateLocation;
using StockFlow.Domain.Locations;

namespace StockFlow.Tests.Application.Locations.UpdateLocation;

public class UpdateLocationHandlerTests
{
    private static UpdateLocationHandler HandlerWith(
        InMemoryLocationWriteRepository repository,
        ICountryReferenceDataService? referenceDataService = null) =>
        new(
            repository,
            referenceDataService ?? new InMemoryCountryReferenceDataService(),
            new InMemoryCurrentUserService(),
            new InMemoryUnitOfWork(),
            new UpdateLocationValidator());

    [Fact]
    public async Task UpdateLocation_WhenItExists_UpdatesLocation()
    {
        var location = Location.Create("WH-100", "Main Warehouse", "123 Main St", null, null, "CA", "Los Angeles", Country.US);
        var repository = new InMemoryLocationWriteRepository([location]);
        var handler = HandlerWith(repository);

        var command = new UpdateLocationCommand(location.Id, "Main Warehouse v2", "456 Other St", null, null, "NY", "New York City", Country.US);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Main Warehouse v2", result.Value.Name);
        Assert.Equal("WH-100", result.Value.Code); // code stays unchanged
        Assert.Equal("New York City", result.Value.City);
    }

    [Fact]
    public async Task UpdateLocation_WhenLocationDoesNotExist_ReturnsNotFound()
    {
        var repository = new InMemoryLocationWriteRepository();
        var handler = HandlerWith(repository);

        var command = new UpdateLocationCommand(Guid.NewGuid(), "Main Warehouse", null, null, null, "CA", "Los Angeles", Country.US);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task UpdateLocation_WhenLocationIsSoftDeleted_ReturnsNotFound()
    {
        var location = Location.Create("WH-100", "Main Warehouse", null, null, null, "CA", "Los Angeles", Country.US);
        location.SoftDelete();
        var repository = new InMemoryLocationWriteRepository([location]);
        var handler = HandlerWith(repository);

        var command = new UpdateLocationCommand(location.Id, "Main Warehouse", null, null, null, "CA", "Los Angeles", Country.US);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task UpdateLocation_WithFullTrioAndAddressLines_UpdatesLocation()
    {
        var location = Location.Create("WH-100", "Main Warehouse", null, null, null, "CA", "Los Angeles", Country.US);
        var repository = new InMemoryLocationWriteRepository([location]);
        var handler = HandlerWith(repository);

        var command = new UpdateLocationCommand(location.Id, "Main Warehouse", "123 Main St", null, null, "CA", "Los Angeles", Country.US);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("123 Main St", result.Value.AddressLine1);
        Assert.Equal("CA", result.Value.State);
        Assert.Equal("Los Angeles", result.Value.City);
    }

    [Fact]
    public async Task UpdateLocation_WithMissingState_ReturnsValidationError()
    {
        var location = Location.Create("WH-100", "Main Warehouse", null, null, null, "CA", "Los Angeles", Country.US);
        var repository = new InMemoryLocationWriteRepository([location]);
        var handler = HandlerWith(repository);

        var command = new UpdateLocationCommand(location.Id, "Main Warehouse", "123 Main St", null, null, string.Empty, "Los Angeles", Country.US);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public async Task UpdateLocation_WithUnrecognizedState_ReturnsValidationError()
    {
        var location = Location.Create("WH-100", "Main Warehouse", null, null, null, "CA", "Los Angeles", Country.US);
        var repository = new InMemoryLocationWriteRepository([location]);
        var handler = HandlerWith(repository);

        var command = new UpdateLocationCommand(location.Id, "Main Warehouse", null, null, null, "ZZ", "Los Angeles", Country.US);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Locations.InvalidState", result.Error.Code);
    }

    [Fact]
    public async Task UpdateLocation_WithUnrecognizedCity_ReturnsValidationError()
    {
        var location = Location.Create("WH-100", "Main Warehouse", null, null, null, "CA", "Los Angeles", Country.US);
        var repository = new InMemoryLocationWriteRepository([location]);
        var handler = HandlerWith(repository);

        var command = new UpdateLocationCommand(location.Id, "Main Warehouse", null, null, null, "CA", "Nowhere", Country.US);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Locations.InvalidCity", result.Error.Code);
    }

    [Fact]
    public async Task UpdateLocation_WhenReferenceDataServiceIsUnavailable_ReturnsUnavailableError()
    {
        var location = Location.Create("WH-100", "Main Warehouse", null, null, null, "CA", "Los Angeles", Country.US);
        var repository = new InMemoryLocationWriteRepository([location]);
        var handler = HandlerWith(repository, new UnavailableCountryReferenceDataService());

        var command = new UpdateLocationCommand(location.Id, "Main Warehouse", null, null, null, "CA", "Los Angeles", Country.US);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unavailable, result.Error.Type);
    }
}
