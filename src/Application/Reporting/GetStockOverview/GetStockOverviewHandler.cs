using StockFlow.Application.Reporting.Shared;

namespace StockFlow.Application.Reporting.GetStockOverview;

public sealed class GetStockOverviewHandler(
    IReportingReadRepository repository,
    IValidator<GetStockOverviewQuery> validator) : IRequestHandler<GetStockOverviewQuery, Result<PagedResult<StockOverviewDto>>>
{
    public async Task<Result<PagedResult<StockOverviewDto>>> Handle(GetStockOverviewQuery request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<PagedResult<StockOverviewDto>>(ReportingErrors.ValidationFailed(validationResult));
        }

        var pagedOverview = await repository.GetStockOverviewAsync(
            request.Page,
            request.PageSize,
            request.LowStockOnly,
            cancellationToken);

        return Result.Success(pagedOverview);
    }
}
