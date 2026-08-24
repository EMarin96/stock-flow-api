using StockFlow.Application.Users.Shared;
using StockFlow.Domain.Users;

namespace StockFlow.Application.Users.CreateUser;

public sealed record CreateUserCommand(string Username, string Password, Role Role) : IRequest<Result<UserDto>>;
