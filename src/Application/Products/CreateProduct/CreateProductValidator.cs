namespace StockFlow.Application.Products.CreateProduct;

public sealed class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(command => command.Sku)
            .NotEmpty()
            .MaximumLength(64);

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
