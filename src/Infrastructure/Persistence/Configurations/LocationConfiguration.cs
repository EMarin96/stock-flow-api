using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockFlow.Domain.Locations;

namespace StockFlow.Infrastructure.Persistence.Configurations;

public sealed class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("Locations");

        builder.HasKey(location => location.Id);

        builder.Property(location => location.Code)
            .HasConversion(code => code.Value, value => LocationCode.Create(value))
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(location => location.Code)
            .IsUnique();

        builder.Property(location => location.Name)
            .HasMaxLength(200)
            .IsRequired();

        // Address is mandatory as a whole (every Location has a country/state/city
        // on file), so it is modeled as an EF Core owned type with a required
        // navigation, same as Product.Price.
        builder.OwnsOne(location => location.Address, addressBuilder =>
        {
            addressBuilder.Property(address => address.AddressLine1)
                .HasColumnName("AddressLine1")
                .HasMaxLength(200);

            addressBuilder.Property(address => address.AddressLine2)
                .HasColumnName("AddressLine2")
                .HasMaxLength(200);

            addressBuilder.Property(address => address.AddressLine3)
                .HasColumnName("AddressLine3")
                .HasMaxLength(200);

            addressBuilder.Property(address => address.State)
                .HasColumnName("State")
                .HasMaxLength(10)
                .IsRequired();

            addressBuilder.Property(address => address.City)
                .HasColumnName("City")
                .HasMaxLength(100)
                .IsRequired();

            // Country is a dependency-free enum stored as its string name, same
            // approach as Product.Price.Currency.
            addressBuilder.Property(address => address.Country)
                .HasColumnName("Country")
                .HasConversion<string>()
                .HasMaxLength(2)
                .IsRequired();
        });

        builder.Navigation(location => location.Address)
            .IsRequired();

        // Soft delete: EF Core queries automatically exclude soft-deleted rows.
        // Dapper read queries do not participate in this filter and must apply
        // "IsDeleted = false" explicitly (see plan.md — Risks).
        builder.HasQueryFilter(location => !location.IsDeleted);
    }
}
