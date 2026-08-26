using StockFlow.Application.Common.Exceptions;
using StockFlow.Application.Common.Security;
using StockFlow.Application.Users.Shared;
using StockFlow.Domain.Users;

namespace StockFlow.Application.Users.CreateUser;

public sealed class CreateUserHandler(
    IUserWriteRepository repository,
    IPasswordHasher passwordHasher,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IValidator<CreateUserCommand> validator) : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<UserDto>(UserErrors.ValidationFailed(validationResult));
        }

        // Pre-check for a clean business error; the DB unique index on
        // NormalizedUsername is the final safety net against the race
        // condition between this check and the insert below (see plan.md —
        // Risks), same dual-guard pattern as SKU/location code.
        var usernameAlreadyExists = await repository.UsernameExistsAsync(request.Username, cancellationToken);
        if (usernameAlreadyExists)
        {
            return Result.Failure<UserDto>(UserErrors.UsernameAlreadyExists(request.Username));
        }

        var passwordHash = passwordHasher.Hash(request.Password);
        var user = User.Create(request.Username, passwordHash, request.Role, currentUserService.UserId);

        try
        {
            await repository.AddAsync(user, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateUniqueConstraintException)
        {
            // Translates a raw DB unique-constraint violation (the race-condition
            // case) into the same business Result the pre-check above returns.
            return Result.Failure<UserDto>(UserErrors.UsernameAlreadyExists(request.Username));
        }

        return Result.Success(user.ToDto());
    }
}
