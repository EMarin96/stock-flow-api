namespace StockFlow.Application.Reporting.GetMovementActivity;

public sealed class GetMovementActivityValidator : AbstractValidator<GetMovementActivityQuery>
{
    public const int MaxPageSize = 100;

    public GetMovementActivityValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be 1 or greater.");

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, MaxPageSize)
            .WithMessage($"Page size must be between 1 and {MaxPageSize}.");

        RuleFor(query => query)
            .Must(query => query.From is null || query.To is null || query.From <= query.To)
            .WithMessage("'From' must not be later than 'To'.");
    }
}
