using StockFlow.Application.Users.Shared;

namespace StockFlow.Application.Users.GetUserById;

public sealed record GetUserByIdQuery(Guid Id) : IRequest<Result<UserDto>>;
