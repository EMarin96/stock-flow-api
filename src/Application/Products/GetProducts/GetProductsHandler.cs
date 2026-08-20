using StockFlow.Application.Products.Shared;

namespace StockFlow.Application.Products.GetProducts;

public sealed class GetProductsHandler(
    IProductReadRepository repository,
    IValidator<GetProductsQuery> validator) : IRequestHandler<GetProductsQuery, Result<PagedResult<ProductDto>>>
{
    public async Task<Result<PagedResult<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<PagedResult<ProductDto>>(ProductErrors.ValidationFailed(validationResult));
        }

        var pagedProducts = await repository.GetPagedAsync(request.Page, request.PageSize, cancellationToken);

        return Result.Success(pagedProducts);
    }
}
