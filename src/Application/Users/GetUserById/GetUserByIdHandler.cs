using StockFlow.Application.Users.Shared;

namespace StockFlow.Application.Users.GetUserById;

public sealed class GetUserByIdHandler(IUserReadRepository repository)
    : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (user is null)
        {
            return Result.Failure<UserDto>(UserErrors.NotFound(request.Id));
        }

        return Result.Success(user);
    }
}
