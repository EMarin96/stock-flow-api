using StockFlow.Infrastructure.Security;

namespace StockFlow.Tests.Infrastructure.Security;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_ThenVerify_WithCorrectPassword_ReturnsTrue()
    {
        var hash = _hasher.Hash("correct-password-123");

        var isValid = _hasher.Verify("correct-password-123", hash);

        Assert.True(isValid);
    }

    [Fact]
    public void Verify_WithWrongPassword_ReturnsFalse()
    {
        var hash = _hasher.Hash("correct-password-123");

        var isValid = _hasher.Verify("wrong-password", hash);

        Assert.False(isValid);
    }

    [Fact]
    public void Hash_CalledTwiceForTheSamePassword_ProducesDifferentHashes()
    {
        // Each call uses a fresh random salt, so two hashes of the same
        // password must never be equal (defense against rainbow tables).
        var firstHash = _hasher.Hash("same-password");
        var secondHash = _hasher.Hash("same-password");

        Assert.NotEqual(firstHash, secondHash);
    }
}
