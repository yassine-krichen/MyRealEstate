using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyRealEstate.Domain.Entities;

namespace MyRealEstate.Infrastructure.Data.Configurations;

public class PropertyViewConfiguration : IEntityTypeConfiguration<PropertyView>
{
    public void Configure(EntityTypeBuilder<PropertyView> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.SessionId)
            .HasMaxLength(100);

        builder.Property(v => v.IpAddress)
            .HasMaxLength(50);

        builder.Property(v => v.UserAgent)
            .HasMaxLength(500);

        // Indexes (as per SCHEMAS.md)
        builder.HasIndex(v => v.PropertyId);
        builder.HasIndex(v => v.ViewedAt);
        builder.HasIndex(v => new { v.PropertyId, v.ViewedAt });
    }
}
