using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyRealEstate.Domain.Entities;

namespace MyRealEstate.Infrastructure.Data.Configurations;

public class PropertyImageConfiguration : IEntityTypeConfiguration<PropertyImage>
{
    public void Configure(EntityTypeBuilder<PropertyImage> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.FilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(i => i.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(i => i.PropertyId);
        builder.HasIndex(i => new { i.PropertyId, i.IsMain });
    }
}
