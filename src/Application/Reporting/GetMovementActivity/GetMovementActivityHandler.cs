using StockFlow.Application.Reporting.Shared;

namespace StockFlow.Application.Reporting.GetMovementActivity;

public sealed class GetMovementActivityHandler(
    IReportingReadRepository repository,
    IValidator<GetMovementActivityQuery> validator) : IRequestHandler<GetMovementActivityQuery, Result<PagedResult<MovementActivityDto>>>
{
    public async Task<Result<PagedResult<MovementActivityDto>>> Handle(GetMovementActivityQuery request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<PagedResult<MovementActivityDto>>(ReportingErrors.ValidationFailed(validationResult));
        }

        var pagedActivity = await repository.GetMovementActivityAsync(
            request.From,
            request.To,
            request.ProductId,
            request.LocationId,
            request.Page,
            request.PageSize,
            cancellationToken);

        return Result.Success(pagedActivity);
    }
}
