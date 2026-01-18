using MyRealEstate.Domain.Enums;
using MyRealEstate.Domain.Interfaces;
using MyRealEstate.Domain.ValueObjects;

namespace MyRealEstate.Domain.Entities;

public class Property : BaseEntity, ISoftDelete
{
    public string Title { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string Description { get; set; } = string.Empty;
    
    // Price as owned value object
    public Money Price { get; set; } = new Money(0);
    
    public string PropertyType { get; set; } = string.Empty; // "Apartment", "House", "Land", etc.
    public PropertyStatus Status { get; set; } = PropertyStatus.Draft;
    
    // Address as owned value object
    public Address Address { get; set; } = null!;
    
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public decimal AreaSqM { get; set; }
    
    // Foreign keys
    public Guid? AgentId { get; set; }
    public User? Agent { get; set; }
    
    public Guid? ClosedDealId { get; set; }
    
    // Cached view count for performance
    public int ViewsCount { get; set; }
    
    // Soft delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    // Navigation properties
    public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
    public ICollection<Inquiry> Inquiries { get; set; } = new List<Inquiry>();
    public ICollection<PropertyView> Views { get; set; } = new List<PropertyView>();

    // Business methods
    public bool CanBePublished()
    {
        return !string.IsNullOrWhiteSpace(Title)
            && !string.IsNullOrWhiteSpace(Description)
            && Price.Amount > 0
            && Address != null
            && !IsDeleted;
    }

    public void Publish()
    {
        if (!CanBePublished())
            throw new InvalidOperationException("Property cannot be published in its current state");
        
        Status = PropertyStatus.Published;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsSold(Guid dealId)
    {
        Status = PropertyStatus.Sold;
        ClosedDealId = dealId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsRented(Guid dealId)
    {
        Status = PropertyStatus.Rented;
        ClosedDealId = dealId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
