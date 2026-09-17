namespace StockFlow.Application.Reporting.GetStockOverview;

public sealed class GetStockOverviewValidator : AbstractValidator<GetStockOverviewQuery>
{
    public const int MaxPageSize = 100;

    public GetStockOverviewValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be 1 or greater.");

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, MaxPageSize)
            .WithMessage($"Page size must be between 1 and {MaxPageSize}.");
    }
}
