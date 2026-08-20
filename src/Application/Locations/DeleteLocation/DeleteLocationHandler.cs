using StockFlow.Application.Locations.Shared;

namespace StockFlow.Application.Locations.DeleteLocation;

public sealed class DeleteLocationHandler(
    ILocationWriteRepository repository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteLocationCommand, Result>
{
    public async Task<Result> Handle(DeleteLocationCommand request, CancellationToken cancellationToken)
    {
        var location = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (location is null)
        {
            return Result.Failure(LocationErrors.NotFound(request.Id));
        }

        location.SoftDelete();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
