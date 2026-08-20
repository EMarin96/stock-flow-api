using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockFlow.Domain.Products;

namespace StockFlow.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(product => product.Id);

        builder.Property(product => product.Sku)
            .HasConversion(sku => sku.Value, value => Sku.Create(value))
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(product => product.Sku)
            .IsUnique();

        builder.Property(product => product.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(product => product.Description)
            .HasMaxLength(1000);

        builder.Property(product => product.UnitOfMeasure)
            .HasMaxLength(32)
            .IsRequired();

        // Money has two components (Amount + Currency), so it is modeled as an
        // EF Core owned type rather than a scalar conversion.
        builder.OwnsOne(product => product.Price, priceBuilder =>
        {
            priceBuilder.Property(price => price.Amount)
                .HasColumnName("Price")
                .HasColumnType("numeric(18,2)");

            priceBuilder.Property(price => price.Currency)
                .HasColumnName("Currency")
                .HasConversion<string>()
                .HasMaxLength(3);
        });

        builder.Navigation(product => product.Price)
            .IsRequired();

        // Soft delete: EF Core queries automatically exclude soft-deleted rows.
        // Dapper read queries do not participate in this filter and must apply
        // "IsDeleted = false" explicitly (see plan.md — Risks).
        builder.HasQueryFilter(product => !product.IsDeleted);
    }
}
