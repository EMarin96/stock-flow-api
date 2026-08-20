using StockFlow.Application.Products.Shared;

namespace StockFlow.Application.Products.UpdateProduct;

public sealed class UpdateProductHandler(
    IProductWriteRepository repository,
    IUnitOfWork unitOfWork,
    IValidator<UpdateProductCommand> validator) : IRequestHandler<UpdateProductCommand, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<ProductDto>(ProductErrors.ValidationFailed(validationResult));
        }

        var product = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
        {
            return Result.Failure<ProductDto>(ProductErrors.NotFound(request.Id));
        }

        product.Update(
            request.Name,
            request.Description,
            request.UnitOfMeasure,
            request.Price,
            request.Currency,
            request.MinimumStockThreshold);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(product.ToDto());
    }
}
