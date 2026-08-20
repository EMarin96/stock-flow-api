using StockFlow.Api.Common;
using StockFlow.Application.Common.Mediator;
using StockFlow.Application.Locations.CreateLocation;
using StockFlow.Application.Locations.DeleteLocation;
using StockFlow.Application.Locations.GetLocationById;
using StockFlow.Application.Locations.GetLocations;
using StockFlow.Application.Locations.Shared;
using StockFlow.Application.Locations.UpdateLocation;
using StockFlow.Domain.Locations;

namespace StockFlow.Api.Endpoints;

public static class LocationEndpoints
{
    public static IEndpointRouteBuilder MapLocationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/locations").WithTags("Locations");

        group.MapPost("/", CreateLocation)
            .WithName("CreateLocation")
            .WithSummary("Creates a new location/warehouse.")
            .Produces<LocationDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);

        group.MapGet("/{id:guid}", GetLocationById)
            .WithName("GetLocationById")
            .WithSummary("Fetches a single location by id.")
            .Produces<LocationDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/", GetLocations)
            .WithName("GetLocations")
            .WithSummary("Lists locations with pagination.")
            .Produces<Application.Common.Pagination.PagedResult<LocationDto>>()
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", UpdateLocation)
            .WithName("UpdateLocation")
            .WithSummary("Updates an existing location. The code is not editable.")
            .Produces<LocationDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);

        group.MapDelete("/{id:guid}", DeleteLocation)
            .WithName("DeleteLocation")
            .WithSummary("Soft-deletes a location.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> CreateLocation(
        CreateLocationRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateLocationCommand(
            request.Code,
            request.Name,
            request.AddressLine1,
            request.AddressLine2,
            request.AddressLine3,
            request.State,
            request.City,
            request.Country);

        var result = await mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.CreatedAtRoute("GetLocationById", new { id = result.Value.Id }, result.Value)
            : result.ToProblem();
    }

    private static async Task<IResult> GetLocationById(Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetLocationByIdQuery(id), cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
    }

    private static async Task<IResult> GetLocations(
        IMediator mediator,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 20)
    {
        var result = await mediator.Send(new GetLocationsQuery(page, pageSize), cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
    }

    private static async Task<IResult> UpdateLocation(
        Guid id,
        UpdateLocationRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new UpdateLocationCommand(
            id,
            request.Name,
            request.AddressLine1,
            request.AddressLine2,
            request.AddressLine3,
            request.State,
            request.City,
            request.Country);

        var result = await mediator.Send(command, cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
    }

    private static async Task<IResult> DeleteLocation(Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteLocationCommand(id), cancellationToken);

        return result.IsSuccess ? Results.NoContent() : result.ToProblem();
    }
}

public sealed record CreateLocationRequest(
    string Code,
    string Name,
    string? AddressLine1,
    string? AddressLine2,
    string? AddressLine3,
    string State,
    string City,
    Country Country);

public sealed record UpdateLocationRequest(
    string Name,
    string? AddressLine1,
    string? AddressLine2,
    string? AddressLine3,
    string State,
    string City,
    Country Country);
