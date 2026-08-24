using StockFlow.Application.Users.Shared;

namespace StockFlow.Application.Users.GetUsers;

public sealed class GetUsersHandler(
    IUserReadRepository repository,
    IValidator<GetUsersQuery> validator) : IRequestHandler<GetUsersQuery, Result<PagedResult<UserDto>>>
{
    public async Task<Result<PagedResult<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<PagedResult<UserDto>>(UserErrors.ValidationFailed(validationResult));
        }

        var pagedUsers = await repository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.Username,
            request.Role,
            cancellationToken);

        return Result.Success(pagedUsers);
    }
}
