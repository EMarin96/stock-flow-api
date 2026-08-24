using StockFlow.Application.Common.Security;
using StockFlow.Application.Users.Shared;

namespace StockFlow.Application.Users.UpdateUser;

public sealed class UpdateUserHandler(
    IUserWriteRepository repository,
    IPasswordHasher passwordHasher,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IValidator<UpdateUserCommand> validator) : IRequestHandler<UpdateUserCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<UserDto>(UserErrors.ValidationFailed(validationResult));
        }

        var user = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (user is null)
        {
            return Result.Failure<UserDto>(UserErrors.NotFound(request.Id));
        }

        user.UpdateRole(request.Role, currentUserService.UserId);

        if (request.NewPassword is not null)
        {
            var passwordHash = passwordHasher.Hash(request.NewPassword);
            user.ResetPassword(passwordHash, currentUserService.UserId);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(user.ToDto());
    }
}
