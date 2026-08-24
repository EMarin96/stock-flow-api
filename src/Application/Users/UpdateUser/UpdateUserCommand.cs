using StockFlow.Application.Users.Shared;
using StockFlow.Domain.Users;

namespace StockFlow.Application.Users.UpdateUser;

// Username is intentionally not part of this command — it is immutable once
// created (see spec.md — Out of scope).
public sealed record UpdateUserCommand(Guid Id, Role Role, string? NewPassword) : IRequest<Result<UserDto>>;
