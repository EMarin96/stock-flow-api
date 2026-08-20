using StockFlow.Application.Products.Shared;

namespace StockFlow.Application.Products.GetProducts;

public sealed record GetProductsQuery(int Page, int PageSize) : IRequest<Result<PagedResult<ProductDto>>>;
