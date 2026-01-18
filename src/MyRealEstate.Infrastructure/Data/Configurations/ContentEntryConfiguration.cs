using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyRealEstate.Domain.Entities;

namespace MyRealEstate.Infrastructure.Data.Configurations;

public class ContentEntryConfiguration : IEntityTypeConfiguration<ContentEntry>
{
    public void Configure(EntityTypeBuilder<ContentEntry> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.HtmlValue)
            .HasMaxLength(50000);

        builder.HasOne(c => c.UpdatedByUser)
            .WithMany()
            .HasForeignKey(c => c.UpdatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Unique index on Key (as per SCHEMAS.md)
        builder.HasIndex(c => c.Key).IsUnique();
    }
}
