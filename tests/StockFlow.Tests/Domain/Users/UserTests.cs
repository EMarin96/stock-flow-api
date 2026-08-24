using StockFlow.Domain.Common;
using StockFlow.Domain.Users;

namespace StockFlow.Tests.Domain.Users;

public class UserTests
{
    [Fact]
    public void Create_WithValidUsernameAndPasswordHash_Succeeds()
    {
        var user = User.Create("alice", "hashed-password", Role.Operator, createdBy: null);

        Assert.Equal("alice", user.Username);
        Assert.Equal("ALICE", user.NormalizedUsername);
        Assert.Equal("hashed-password", user.PasswordHash);
        Assert.Equal(Role.Operator, user.Role);
        Assert.False(user.IsDeleted);
    }

    [Fact]
    public void Create_NormalizesUsernameToUpperInvariant()
    {
        var user = User.Create("MiXeDCaSe", "hashed-password", Role.ReadOnly, createdBy: null);

        Assert.Equal("MIXEDCASE", user.NormalizedUsername);
    }

    [Fact]
    public void Create_SetsCreatedAtAndCreatedBy()
    {
        var actingUserId = Guid.NewGuid();

        var user = User.Create("alice", "hashed-password", Role.Admin, actingUserId);

        Assert.True(user.CreatedAt <= DateTime.UtcNow);
        Assert.Equal(actingUserId, user.CreatedBy);
    }

    [Fact]
    public void Create_WithEmptyUsername_Throws()
    {
        Assert.Throws<DomainValidationException>(() => User.Create(string.Empty, "hashed-password", Role.Admin, null));
    }

    [Fact]
    public void Create_WithUsernameLongerThan100Characters_Throws()
    {
        var tooLong = new string('a', 101);

        Assert.Throws<DomainValidationException>(() => User.Create(tooLong, "hashed-password", Role.Admin, null));
    }

    [Fact]
    public void Create_WithEmptyPasswordHash_Throws()
    {
        Assert.Throws<DomainValidationException>(() => User.Create("alice", string.Empty, Role.Admin, null));
    }

    [Fact]
    public void UpdateRole_ChangesRoleAndSetsUpdatedFields()
    {
        var user = User.Create("alice", "hashed-password", Role.ReadOnly, null);
        var actingUserId = Guid.NewGuid();

        user.UpdateRole(Role.Admin, actingUserId);

        Assert.Equal(Role.Admin, user.Role);
        Assert.NotNull(user.UpdatedAt);
        Assert.Equal(actingUserId, user.UpdatedBy);
    }

    [Fact]
    public void ResetPassword_ChangesPasswordHashAndSetsUpdatedFields()
    {
        var user = User.Create("alice", "old-hash", Role.Operator, null);
        var actingUserId = Guid.NewGuid();

        user.ResetPassword("new-hash", actingUserId);

        Assert.Equal("new-hash", user.PasswordHash);
        Assert.NotNull(user.UpdatedAt);
        Assert.Equal(actingUserId, user.UpdatedBy);
    }

    [Fact]
    public void ResetPassword_WithEmptyHash_Throws()
    {
        var user = User.Create("alice", "old-hash", Role.Operator, null);

        Assert.Throws<DomainValidationException>(() => user.ResetPassword(string.Empty, null));
    }

    [Fact]
    public void SoftDelete_MarksUserAsDeleted()
    {
        var user = User.Create("alice", "hashed-password", Role.Operator, null);

        user.SoftDelete();

        Assert.True(user.IsDeleted);
        Assert.NotNull(user.DeletedAt);
    }
}
