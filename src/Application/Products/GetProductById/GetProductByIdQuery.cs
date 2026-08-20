using StockFlow.Application.Products.Shared;

namespace StockFlow.Application.Products.GetProductById;

public sealed record GetProductByIdQuery(Guid Id) : IRequest<Result<ProductDto>>;
