using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockFlow.Domain.Locations;
using StockFlow.Domain.Products;
using StockFlow.Domain.StockMovements;

namespace StockFlow.Infrastructure.Persistence.Configurations;

public sealed class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");

        builder.HasKey(movement => movement.Id);

        builder.Property(movement => movement.Type)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(movement => movement.Quantity)
            .IsRequired();

        builder.Property(movement => movement.Direction)
            .HasConversion<string>()
            .HasMaxLength(16);

        builder.Property(movement => movement.CreatedAt)
            .IsRequired();

        // No navigation properties on StockMovement (references Product/Location
        // by id only, see plan.md), so both relationships are configured without
        // a navigation expression; EF Core distinguishes them by foreign key.
        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(movement => movement.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Location>()
            .WithMany()
            .HasForeignKey(movement => movement.SourceLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Location>()
            .WithMany()
            .HasForeignKey(movement => movement.DestinationLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        // No soft-delete flag/global query filter and no Update repository
        // method — movements are append-only, never edited or deleted (see
        // constitution/tech-stack.md — Hard limits).
    }
}
