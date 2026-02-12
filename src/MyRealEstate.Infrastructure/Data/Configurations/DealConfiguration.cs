using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyRealEstate.Domain.Entities;

namespace MyRealEstate.Infrastructure.Data.Configurations;

public class DealConfiguration : IEntityTypeConfiguration<Deal>
{
    public void Configure(EntityTypeBuilder<Deal> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.SalePrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(d => d.CommissionPercent)
            .HasPrecision(5, 2);

        builder.Property(d => d.CommissionAmount)
            .HasPrecision(18, 2);

        builder.Property(d => d.BuyerName)
            .HasMaxLength(200);

        builder.Property(d => d.BuyerEmail)
            .HasMaxLength(255);

        builder.Property(d => d.BuyerPhone)
            .HasMaxLength(50);

        builder.Property(d => d.Notes)
            .HasMaxLength(2000);

        // Relationships
        builder.HasOne(d => d.Property)
            .WithMany()
            .HasForeignKey(d => d.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Agent)
            .WithMany()
            .HasForeignKey(d => d.AgentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Inquiry)
            .WithMany()
            .HasForeignKey(d => d.InquiryId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes (as per SCHEMAS.md)
        builder.HasIndex(d => d.AgentId);
        builder.HasIndex(d => d.ClosedAt);
        builder.HasIndex(d => d.Status);
        builder.HasIndex(d => new { d.AgentId, d.ClosedAt });
    }
}
