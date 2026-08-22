using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockFlow.Domain.Locations;
using StockFlow.Domain.Products;
using StockFlow.Domain.StockMovements;

namespace StockFlow.Infrastructure.Persistence.Configurations;

public sealed class StockLevelConfiguration : IEntityTypeConfiguration<StockLevel>
{
    public const string QuantityCheckConstraintName = "CK_StockLevels_Quantity_NonNegative";

    public void Configure(EntityTypeBuilder<StockLevel> builder)
    {
        // DB-level CHECK (Quantity >= 0), the final safety net against races
        // (same role as the SKU unique index in 001 — see plan.md — Decisions).
        // A check-constraint violation here is translated by IUnitOfWork into
        // StockMovementErrors.InsufficientStock.
        builder.ToTable("StockLevels", table => table.HasCheckConstraint(QuantityCheckConstraintName, "\"Quantity\" >= 0"));

        builder.HasKey(stockLevel => stockLevel.Id);

        builder.Property(stockLevel => stockLevel.Quantity)
            .IsRequired();

        // One StockLevel row per product+location pair.
        builder.HasIndex(stockLevel => new { stockLevel.ProductId, stockLevel.LocationId })
            .IsUnique();

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(stockLevel => stockLevel.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Location>()
            .WithMany()
            .HasForeignKey(stockLevel => stockLevel.LocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
