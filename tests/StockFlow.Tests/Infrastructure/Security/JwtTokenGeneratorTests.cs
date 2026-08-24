using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using StockFlow.Domain.Users;
using StockFlow.Infrastructure.Security;

namespace StockFlow.Tests.Infrastructure.Security;

public class JwtTokenGeneratorTests
{
    private static JwtTokenGenerator NewGenerator(int expiryMinutes = 480) =>
        new(Options.Create(new JwtOptions
        {
            Secret = "unit-test-signing-key-not-for-production-use-only-32chars+",
            Issuer = "StockFlowApi",
            Audience = "StockFlowApi",
            ExpiryMinutes = expiryMinutes,
        }));

    [Fact]
    public void GenerateToken_IncludesUserIdUsernameAndRoleClaims()
    {
        var generator = NewGenerator();
        var userId = Guid.NewGuid();

        var (token, _) = generator.GenerateToken(userId, "alice", Role.Operator);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal(userId.ToString(), jwt.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value);
        Assert.Equal("alice", jwt.Claims.First(claim => claim.Type == ClaimTypes.Name).Value);
        Assert.Equal(nameof(Role.Operator), jwt.Claims.First(claim => claim.Type == ClaimTypes.Role).Value);
    }

    [Fact]
    public void GenerateToken_SetsIssuerAndAudienceFromOptions()
    {
        var generator = NewGenerator();

        var (token, _) = generator.GenerateToken(Guid.NewGuid(), "alice", Role.Admin);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal("StockFlowApi", jwt.Issuer);
        Assert.Contains("StockFlowApi", jwt.Audiences);
    }

    [Fact]
    public void GenerateToken_ExpiresAtMatchesConfiguredExpiryMinutes()
    {
        var generator = NewGenerator(expiryMinutes: 60);

        var (_, expiresAt) = generator.GenerateToken(Guid.NewGuid(), "alice", Role.ReadOnly);

        Assert.True(expiresAt <= DateTime.UtcNow.AddMinutes(60).AddSeconds(5));
        Assert.True(expiresAt >= DateTime.UtcNow.AddMinutes(60).AddSeconds(-5));
    }
}
