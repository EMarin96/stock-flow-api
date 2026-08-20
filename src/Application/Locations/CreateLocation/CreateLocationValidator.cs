namespace StockFlow.Application.Locations.CreateLocation;

public sealed class CreateLocationValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationValidator()
    {
        RuleFor(command => command.Code)
            .NotEmpty()
            .MaximumLength(32);

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(command => command.AddressLine1)
            .MaximumLength(200);

        RuleFor(command => command.AddressLine2)
            .MaximumLength(200);

        RuleFor(command => command.AddressLine3)
            .MaximumLength(200);

        RuleFor(command => command.State)
            .NotEmpty()
            .MaximumLength(10);

        RuleFor(command => command.City)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.Country)
            .IsInEnum();
    }
}
