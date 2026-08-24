using StockFlow.Domain.Users;

namespace StockFlow.Application.Users.Shared;

/// <summary>
/// A dedicated projection for the login flow only — the only code path that
/// ever reads <see cref="PasswordHash"/> (see plan.md — Decisions). Kept
/// separate from <see cref="UserDto"/> so a password hash can never
/// accidentally end up serialized in a Users CRUD response.
/// </summary>
public sealed record AuthenticationRecord(
    Guid Id,
    string Username,
    string PasswordHash,
    Role Role,
    bool IsDeleted);
