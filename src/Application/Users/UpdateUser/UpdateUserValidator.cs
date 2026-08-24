namespace StockFlow.Application.Users.UpdateUser;

public sealed class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();

        RuleFor(command => command.Role)
            .IsInEnum();

        RuleFor(command => command.NewPassword)
            .MinimumLength(8)
            .When(command => command.NewPassword is not null);
    }
}
