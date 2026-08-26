using StockFlow.Application.Users.Shared;
using StockFlow.Domain.Users;

namespace StockFlow.Application.Users.GetUsers;

public sealed record GetUsersQuery(
    int Page,
    int PageSize,
    string? Username,
    Role? Role) : IRequest<Result<PagedResult<UserDto>>>;
