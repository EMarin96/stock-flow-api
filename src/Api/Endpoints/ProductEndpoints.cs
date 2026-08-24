using StockFlow.Api.Common;
using StockFlow.Application.Common.Mediator;
using StockFlow.Application.Products.CreateProduct;
using StockFlow.Application.Products.DeleteProduct;
using StockFlow.Application.Products.GetProductById;
using StockFlow.Application.Products.GetProducts;
using StockFlow.Application.Products.Shared;
using StockFlow.Application.Products.UpdateProduct;
using StockFlow.Domain.Products;

namespace StockFlow.Api.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        group.MapPost("/", CreateProduct)
            .WithName("CreateProduct")
            .WithSummary("Creates a new product.")
            .Produces<ProductDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization("WriteAccess");

        group.MapGet("/{id:guid}", GetProductById)
            .WithName("GetProductById")
            .WithSummary("Fetches a single product by id.")
            .Produces<ProductDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/", GetProducts)
            .WithName("GetProducts")
            .WithSummary("Lists products with pagination.")
            .Produces<Application.Common.Pagination.PagedResult<ProductDto>>()
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", UpdateProduct)
            .WithName("UpdateProduct")
            .WithSummary("Updates an existing product. The SKU is not editable.")
            .Produces<ProductDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization("WriteAccess");

        group.MapDelete("/{id:guid}", DeleteProduct)
            .WithName("DeleteProduct")
            .WithSummary("Soft-deletes a product.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization("AdminOnly");

        return app;
    }

    private static async Task<IResult> CreateProduct(
        CreateProductRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand(
            request.Sku,
            request.Name,
            request.Description,
            request.UnitOfMeasure,
            request.Price,
            request.Currency,
            request.MinimumStockThreshold);

        var result = await mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.CreatedAtRoute("GetProductById", new { id = result.Value.Id }, result.Value)
            : result.ToProblem();
    }

    private static async Task<IResult> GetProductById(Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetProductByIdQuery(id), cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
    }

    private static async Task<IResult> GetProducts(
        IMediator mediator,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 20)
    {
        var result = await mediator.Send(new GetProductsQuery(page, pageSize), cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
    }

    private static async Task<IResult> UpdateProduct(
        Guid id,
        UpdateProductRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProductCommand(
            id,
            request.Name,
            request.Description,
            request.UnitOfMeasure,
            request.Price,
            request.Currency,
            request.MinimumStockThreshold);

        var result = await mediator.Send(command, cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
    }

    private static async Task<IResult> DeleteProduct(Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteProductCommand(id), cancellationToken);

        return result.IsSuccess ? Results.NoContent() : result.ToProblem();
    }
}

public sealed record CreateProductRequest(
    string Sku,
    string Name,
    string? Description,
    string UnitOfMeasure,
    decimal Price,
    Currency Currency,
    int MinimumStockThreshold);

public sealed record UpdateProductRequest(
    string Name,
    string? Description,
    string UnitOfMeasure,
    decimal Price,
    Currency Currency,
    int MinimumStockThreshold);
