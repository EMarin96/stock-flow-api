using StockFlow.Application.Products.Shared;

namespace StockFlow.Application.Products.GetProductById;

public sealed class GetProductByIdHandler(IProductReadRepository repository)
    : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
        {
            return Result.Failure<ProductDto>(ProductErrors.NotFound(request.Id));
        }

        return Result.Success(product);
    }
}
