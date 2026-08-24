namespace StockFlow.Application.Users.CreateUser;

public sealed class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(command => command.Username)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.Password)
            .NotEmpty()
            .MinimumLength(8);

        RuleFor(command => command.Role)
            .IsInEnum();
    }
}
