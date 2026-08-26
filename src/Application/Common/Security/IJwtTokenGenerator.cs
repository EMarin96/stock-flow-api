using StockFlow.Domain.Users;

namespace StockFlow.Application.Common.Security;

/// <summary>
/// Issues signed JWTs for an authenticated user. Implemented in Infrastructure
/// (System.IdentityModel.Tokens.Jwt, see plan.md — Decisions) so Application
/// stays free of token-library specifics.
/// </summary>
public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) GenerateToken(Guid userId, string username, Role role);
}
