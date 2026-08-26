using StockFlow.Application.Common.Security;
using StockFlow.Application.Users.Shared;

namespace StockFlow.Application.Users.DeactivateUser;

public sealed class DeactivateUserHandler(
    IUserWriteRepository repository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork) : IRequestHandler<DeactivateUserCommand, Result>
{
    public async Task<Result> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(request.Id));
        }

        // Prevents locking out the only authenticated admin in the session
        // (see spec.md — acceptance criteria).
        if (request.Id == currentUserService.UserId)
        {
            return Result.Failure(UserErrors.CannotDeactivateSelf());
        }

        user.SoftDelete();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
