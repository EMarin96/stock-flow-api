using StockFlow.Api.Common;
using StockFlow.Api.Security;
using StockFlow.Application.Common.Mediator;
using StockFlow.Application.StockMovements.CreateStockMovement;
using StockFlow.Application.StockMovements.GetStockMovements;
using StockFlow.Application.StockMovements.Shared;
using StockFlow.Domain.StockMovements;

namespace StockFlow.Api.Endpoints;

public static class StockMovementEndpoints
{
    public static IEndpointRouteBuilder MapStockMovementEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/stock-movements").WithTags("StockMovements");

        group.MapPost("/", CreateStockMovement)
            .WithName("CreateStockMovement")
            .WithSummary("Registers a new stock movement (IN/OUT/TRANSFER/ADJUSTMENT), updating the affected location(s)' current stock atomically.")
            .Produces<StockMovementDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization(nameof(AuthorizationPolicy.WriteAccess));

        group.MapGet("/", GetStockMovements)
            .WithName("GetStockMovements")
            .WithSummary("Lists stock movements with pagination, optionally filtered by product, location (matching either source or destination), and/or type.")
            .Produces<Application.Common.Pagination.PagedResult<StockMovementDto>>()
            .ProducesValidationProblem();

        return app;
    }

    private static async Task<IResult> CreateStockMovement(
        CreateStockMovementRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateStockMovementCommand(
            request.ProductId,
            request.Type,
            request.Quantity,
            request.SourceLocationId,
            request.DestinationLocationId,
            request.Direction);

        var result = await mediator.Send(command, cancellationToken);

        // No GetStockMovementById endpoint exists (out of scope, see plan.md), so
        // the Location header is built directly rather than via CreatedAtRoute.
        return result.IsSuccess
            ? Results.Created($"/api/stock-movements/{result.Value.Id}", result.Value)
            : result.ToProblem();
    }

    private static async Task<IResult> GetStockMovements(
        IMediator mediator,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 20,
        Guid? productId = null,
        Guid? locationId = null,
        MovementType? type = null)
    {
        var result = await mediator.Send(new GetStockMovementsQuery(page, pageSize, productId, locationId, type), cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
    }
}

public sealed record CreateStockMovementRequest(
    Guid ProductId,
    MovementType Type,
    int Quantity,
    Guid? SourceLocationId,
    Guid? DestinationLocationId,
    MovementDirection? Direction);
