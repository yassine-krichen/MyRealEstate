using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyRealEstate.Domain.Entities;

namespace MyRealEstate.Infrastructure.Data.Configurations;

public class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Slug)
            .HasMaxLength(250);

        builder.Property(p => p.Description)
            .HasMaxLength(5000);

        builder.Property(p => p.PropertyType)
            .IsRequired()
            .HasMaxLength(50);

        // Configure owned entity Money
        builder.OwnsOne(p => p.Price, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Price")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Configure owned entity Address
        builder.OwnsOne(p => p.Address, address =>
        {
            address.Property(a => a.Line1)
                .HasColumnName("AddressLine1")
                .HasMaxLength(200)
                .IsRequired();

            address.Property(a => a.Line2)
                .HasColumnName("AddressLine2")
                .HasMaxLength(200);

            address.Property(a => a.City)
                .HasColumnName("City")
                .HasMaxLength(100)
                .IsRequired();

            address.Property(a => a.State)
                .HasColumnName("State")
                .HasMaxLength(100);

            address.Property(a => a.PostalCode)
                .HasColumnName("PostalCode")
                .HasMaxLength(20);

            address.Property(a => a.Country)
                .HasColumnName("Country")
                .HasMaxLength(100)
                .IsRequired();

            address.Property(a => a.Latitude)
                .HasColumnName("Latitude")
                .HasPrecision(10, 7);

            address.Property(a => a.Longitude)
                .HasColumnName("Longitude")
                .HasPrecision(10, 7);
        });

        builder.Property(p => p.AreaSqM)
            .HasPrecision(10, 2);

        // Relationships
        builder.HasOne(p => p.Agent)
            .WithMany()
            .HasForeignKey(p => p.AgentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.Images)
            .WithOne(i => i.Property)
            .HasForeignKey(i => i.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Inquiries)
            .WithOne(i => i.Property)
            .HasForeignKey(i => i.PropertyId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.Views)
            .WithOne(v => v.Property)
            .HasForeignKey(v => v.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for performance
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.AgentId);
        builder.HasIndex(p => new { p.Status, p.IsDeleted });
        builder.HasIndex(p => p.Slug).IsUnique();
    }
}
