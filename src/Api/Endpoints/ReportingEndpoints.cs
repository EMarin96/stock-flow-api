using StockFlow.Api.Common;
using StockFlow.Application.Common.Mediator;
using StockFlow.Application.Common.Pagination;
using StockFlow.Application.Reporting.GetMovementActivity;
using StockFlow.Application.Reporting.GetStockOverview;
using StockFlow.Application.Reporting.Shared;

namespace StockFlow.Api.Endpoints;

public static class ReportingEndpoints
{
    public static IEndpointRouteBuilder MapReportingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reports").WithTags("Reporting");

        group.MapGet("/stock-overview", GetStockOverview)
            .WithName("GetStockOverview")
            .WithSummary("Total stock per active product, summed across every location, with a low-stock flag.")
            .Produces<PagedResult<StockOverviewDto>>()
            .ProducesValidationProblem();

        group.MapGet("/movement-activity", GetMovementActivity)
            .WithName("GetMovementActivity")
            .WithSummary("Stock movements grouped by product and type within an optional date range.")
            .Produces<PagedResult<MovementActivityDto>>()
            .ProducesValidationProblem();

        return app;
    }

    private static async Task<IResult> GetStockOverview(
        IMediator mediator,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 20,
        bool? lowStockOnly = null)
    {
        var result = await mediator.Send(new GetStockOverviewQuery(page, pageSize, lowStockOnly), cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
    }

    private static async Task<IResult> GetMovementActivity(
        IMediator mediator,
        CancellationToken cancellationToken,
        DateTime? from = null,
        DateTime? to = null,
        Guid? productId = null,
        Guid? locationId = null,
        int page = 1,
        int pageSize = 20)
    {
        var result = await mediator.Send(
            new GetMovementActivityQuery(from, to, productId, locationId, page, pageSize),
            cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
    }
}
