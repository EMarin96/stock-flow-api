namespace StockFlow.Application.Products.UpdateProduct;

public sealed class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(command => command.Description)
            .MaximumLength(1000);

        RuleFor(command => command.UnitOfMeasure)
            .NotEmpty()
            .MaximumLength(32);

        RuleFor(command => command.Price)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Price cannot be negative.");

        RuleFor(command => command.Currency)
            .IsInEnum();

        RuleFor(command => command.MinimumStockThreshold)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Minimum stock threshold cannot be negative.");
    }
}
