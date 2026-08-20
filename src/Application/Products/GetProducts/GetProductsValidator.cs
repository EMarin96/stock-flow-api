namespace StockFlow.Application.Products.GetProducts;

public sealed class GetProductsValidator : AbstractValidator<GetProductsQuery>
{
    public const int MaxPageSize = 100;

    public GetProductsValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be 1 or greater.");

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, MaxPageSize)
            .WithMessage($"Page size must be between 1 and {MaxPageSize}.");
    }
}
