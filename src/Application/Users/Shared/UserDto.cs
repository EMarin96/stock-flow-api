using StockFlow.Domain.Users;

namespace StockFlow.Application.Users.Shared;

// Deliberately carries no password/password-hash field — see plan.md
// (Decisions — GetForAuthenticationAsync returns a dedicated record).
public sealed record UserDto(
    Guid Id,
    string Username,
    Role Role,
    DateTime CreatedAt,
    Guid? CreatedBy,
    DateTime? UpdatedAt,
    Guid? UpdatedBy);
