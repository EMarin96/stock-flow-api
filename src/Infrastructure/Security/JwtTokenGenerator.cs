using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StockFlow.Application.Common.Security;
using StockFlow.Domain.Users;

namespace StockFlow.Infrastructure.Security;

/// <summary>
/// Issues signed JWTs via <see cref="JwtSecurityTokenHandler"/>. Claims use
/// <see cref="ClaimTypes.NameIdentifier"/>/<see cref="ClaimTypes.Role"/>/
/// <see cref="ClaimTypes.Name"/> so ASP.NET Core's
/// <c>[Authorize(Roles=...)]</c>/<c>RequireRole</c> work with zero extra
/// claim-type mapping (see plan.md — Implementation).
/// </summary>
public sealed class JwtTokenGenerator(IOptions<JwtOptions> options) : IJwtTokenGenerator
{
    public (string Token, DateTime ExpiresAt) GenerateToken(Guid userId, string username, Role role)
    {
        var jwtOptions = options.Value;
        var expiresAt = DateTime.UtcNow.AddMinutes(jwtOptions.ExpiryMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role.ToString()),
        };

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: signingCredentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
