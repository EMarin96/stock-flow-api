namespace StockFlow.Application.Locations.GetLocations;

public sealed class GetLocationsValidator : AbstractValidator<GetLocationsQuery>
{
    public const int MaxPageSize = 100;

    public GetLocationsValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be 1 or greater.");

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, MaxPageSize)
            .WithMessage($"Page size must be between 1 and {MaxPageSize}.");
    }
}
