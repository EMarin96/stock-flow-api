using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockFlow.Domain.Users;

namespace StockFlow.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Username)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(user => user.NormalizedUsername)
            .HasMaxLength(100)
            .IsRequired();

        // Case-insensitive uniqueness via a normalized column, not a
        // Postgres-specific `citext` extension (see plan.md — Decisions).
        builder.HasIndex(user => user.NormalizedUsername)
            .IsUnique();

        builder.Property(user => user.PasswordHash)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(user => user.Role)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        // Soft delete (deactivation): EF Core queries automatically exclude
        // soft-deleted rows. Dapper read queries do not participate in this
        // filter and must apply "IsDeleted = false" explicitly (see plan.md —
        // Risks), except GetForAuthenticationAsync (see plan.md — Decisions).
        builder.HasQueryFilter(user => !user.IsDeleted);
    }
}
