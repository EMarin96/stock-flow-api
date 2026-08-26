using StockFlow.Application.Locations.CreateLocation;
using StockFlow.Application.Locations.Shared;
using StockFlow.Domain.Locations;

namespace StockFlow.Tests.Application.Locations.CreateLocation;

public class CreateLocationHandlerTests
{
    private static CreateLocationCommand ValidCommand(string code = "WH-100") => new(
        Code: code,
        Name: "Main Warehouse",
        AddressLine1: "123 Main St",
        AddressLine2: null,
        AddressLine3: null,
        State: "CA",
        City: "Los Angeles",
        Country: Country.US);

    private static CreateLocationHandler HandlerWith(
        InMemoryLocationWriteRepository? repository = null,
        ICountryReferenceDataService? referenceDataService = null,
        InMemoryCurrentUserService? currentUserService = null) =>
        new(
            repository ?? new InMemoryLocationWriteRepository(),
            referenceDataService ?? new InMemoryCountryReferenceDataService(),
            currentUserService ?? new InMemoryCurrentUserService(),
            new InMemoryUnitOfWork(),
            new CreateLocationValidator());

    [Fact]
    public async Task CreateLocation_WhenCommandIsValidAndCodeIsUnique_CreatesLocation()
    {
        var handler = HandlerWith();

        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("WH-100", result.Value.Code);
        Assert.Equal("Los Angeles", result.Value.City);
    }

    [Fact]
    public async Task CreateLocation_SetsCreatedByToTheAuthenticatedUser()
    {
        var actingUserId = Guid.NewGuid();
        var handler = HandlerWith(currentUserService: new InMemoryCurrentUserService(actingUserId));

        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(actingUserId, result.Value.CreatedBy);
    }

    [Fact]
    public async Task CreateLocation_WhenCodeAlreadyExists_ReturnsConflict()
    {
        var existingLocation = Location.Create("WH-100", "Existing", null, null, null, "CA", "Los Angeles", Country.US);
        var handler = HandlerWith(new InMemoryLocationWriteRepository([existingLocation]));

        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public async Task CreateLocation_WhenCommandIsInvalid_ReturnsValidationError()
    {
        var handler = HandlerWith();

        var result = await handler.Handle(ValidCommand() with { Code = string.Empty }, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public async Task CreateLocation_WithFullTrioAndAddressLines_CreatesLocation()
    {
        var handler = HandlerWith();

        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("123 Main St", result.Value.AddressLine1);
        Assert.Equal("CA", result.Value.State);
        Assert.Equal("Los Angeles", result.Value.City);
        Assert.Equal(Country.US, result.Value.Country);
    }

    [Fact]
    public async Task CreateLocation_WithFullTrioAndNoAddressLines_CreatesLocation()
    {
        var handler = HandlerWith();

        var command = ValidCommand() with { AddressLine1 = null, AddressLine2 = null, AddressLine3 = null };
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.AddressLine1);
        Assert.Equal("CA", result.Value.State);
        Assert.Equal("Los Angeles", result.Value.City);
    }

    [Fact]
    public async Task CreateLocation_WithAddressLineButMissingState_ReturnsValidationError()
    {
        var handler = HandlerWith();

        var command = ValidCommand() with { State = string.Empty };
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public async Task CreateLocation_WhenCityIsMissing_ReturnsValidationError()
    {
        var handler = HandlerWith();

        var command = ValidCommand() with { City = string.Empty };
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public async Task CreateLocation_WithUnrecognizedState_ReturnsValidationError()
    {
        var handler = HandlerWith();

        var command = ValidCommand() with { State = "ZZ" };
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal("Locations.InvalidState", result.Error.Code);
    }

    [Fact]
    public async Task CreateLocation_WithUnrecognizedCity_ReturnsValidationError()
    {
        var handler = HandlerWith();

        var command = ValidCommand() with { City = "Nowhere" };
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal("Locations.InvalidCity", result.Error.Code);
    }

    [Fact]
    public async Task CreateLocation_WhenReferenceDataServiceIsUnavailable_ReturnsUnavailableError()
    {
        var handler = HandlerWith(referenceDataService: new UnavailableCountryReferenceDataService());

        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unavailable, result.Error.Type);
        Assert.Equal("Locations.ReferenceDataUnavailable", result.Error.Code);
    }
}
