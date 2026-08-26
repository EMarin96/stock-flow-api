namespace StockFlow.Application.Auth.Login;

public sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(command => command.Username)
            .NotEmpty();

        RuleFor(command => command.Password)
            .NotEmpty();
    }
}
