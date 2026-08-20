namespace StockFlow.Application.Products.Shared;

public static class ProductErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Products.NotFound", $"No product was found with id '{id}'.");

    public static Error SkuAlreadyExists(string sku) =>
        Error.Conflict("Products.SkuAlreadyExists", $"A product with SKU '{sku}' already exists.");

    public static Error ValidationFailed(FluentValidation.Results.ValidationResult validationResult) =>
        Error.Validation(
            "Products.ValidationFailed",
            string.Join(" ", validationResult.Errors.Select(failure => failure.ErrorMessage)));
}
