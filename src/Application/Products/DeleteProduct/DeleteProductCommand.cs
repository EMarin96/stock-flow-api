namespace StockFlow.Application.Products.DeleteProduct;

public sealed record DeleteProductCommand(Guid Id) : IRequest<Result>;
