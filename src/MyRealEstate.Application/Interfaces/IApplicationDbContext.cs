using Microsoft.EntityFrameworkCore;
using MyRealEstate.Domain.Entities;

// DI: decouples your application logic from EF Core implementation
// Application defines what it needs & Infrastructure figures out how to provide it
// Pros: testability, maintainability, separation of concerns
namespace MyRealEstate.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Property> Properties { get; }
    DbSet<PropertyImage> PropertyImages { get; }
    DbSet<Inquiry> Inquiries { get; }
    DbSet<ConversationMessage> ConversationMessages { get; }
    DbSet<Deal> Deals { get; }
    DbSet<PropertyView> PropertyViews { get; }
    DbSet<ContentEntry> ContentEntries { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
}
