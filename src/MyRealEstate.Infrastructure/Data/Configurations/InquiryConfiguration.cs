using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyRealEstate.Domain.Entities;

namespace MyRealEstate.Infrastructure.Data.Configurations;

public class InquiryConfiguration : IEntityTypeConfiguration<Inquiry>
{
    public void Configure(EntityTypeBuilder<Inquiry> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.VisitorName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.VisitorEmail)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(i => i.VisitorPhone)
            .HasMaxLength(50);

        builder.Property(i => i.InitialMessage)
            .IsRequired()
            .HasMaxLength(2000);

        // Relationships
        builder.HasOne(i => i.AssignedAgent)
            .WithMany()
            .HasForeignKey(i => i.AssignedAgentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(i => i.Messages)
            .WithOne(m => m.Inquiry)
            .HasForeignKey(m => m.InquiryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for performance (as per SCHEMAS.md)
        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => i.AssignedAgentId);
        builder.HasIndex(i => i.CreatedAt);
        builder.HasIndex(i => new { i.AssignedAgentId, i.Status, i.CreatedAt });
    }
}
