using StockFlow.Application.Common.Exceptions;
using StockFlow.Application.Products.Shared;
using StockFlow.Domain.Products;

namespace StockFlow.Application.Products.CreateProduct;

public sealed class CreateProductHandler(
    IProductWriteRepository repository,
    IUnitOfWork unitOfWork,
    IValidator<CreateProductCommand> validator) : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<ProductDto>(ProductErrors.ValidationFailed(validationResult));
        }

        // Pre-check for a clean business error; the DB unique index on Sku is the
        // final safety net against the race condition between this check and the
        // insert below (see plan.md — Risks).
        var skuAlreadyExists = await repository.SkuExistsAsync(request.Sku, cancellationToken);
        if (skuAlreadyExists)
        {
            return Result.Failure<ProductDto>(ProductErrors.SkuAlreadyExists(request.Sku));
        }

        var product = Product.Create(
            request.Sku,
            request.Name,
            request.Description,
            request.UnitOfMeasure,
            request.Price,
            request.Currency,
            request.MinimumStockThreshold);

        try
        {
            await repository.AddAsync(product, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateUniqueConstraintException)
        {
            // Translates a raw DB unique-constraint violation (the race-condition
            // case) into the same business Result the pre-check above returns.
            return Result.Failure<ProductDto>(ProductErrors.SkuAlreadyExists(request.Sku));
        }

        return Result.Success(product.ToDto());
    }
}
