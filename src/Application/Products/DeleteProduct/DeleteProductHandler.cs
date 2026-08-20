using StockFlow.Application.Products.Shared;

namespace StockFlow.Application.Products.DeleteProduct;

public sealed class DeleteProductHandler(
    IProductWriteRepository repository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteProductCommand, Result>
{
    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
        {
            return Result.Failure(ProductErrors.NotFound(request.Id));
        }

        product.SoftDelete();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
