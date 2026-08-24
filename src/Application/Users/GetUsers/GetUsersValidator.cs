namespace StockFlow.Application.Users.GetUsers;

public sealed class GetUsersValidator : AbstractValidator<GetUsersQuery>
{
    public const int MaxPageSize = 100;

    public GetUsersValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be 1 or greater.");

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, MaxPageSize)
            .WithMessage($"Page size must be between 1 and {MaxPageSize}.");

        RuleFor(query => query.Role)
            .IsInEnum()
            .When(query => query.Role is not null);
    }
}
