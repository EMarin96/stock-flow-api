namespace StockFlow.Application.StockMovements.GetStockMovements;

public sealed class GetStockMovementsValidator : AbstractValidator<GetStockMovementsQuery>
{
    public const int MaxPageSize = 100;

    public GetStockMovementsValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be 1 or greater.");

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, MaxPageSize)
            .WithMessage($"Page size must be between 1 and {MaxPageSize}.");

        RuleFor(query => query.Type)
            .IsInEnum()
            .When(query => query.Type is not null);
    }
}
