namespace MyRealEstate.Domain.Entities;

// To track how many times a property has been viewed (popularity).
// For analytics: “Top viewed properties,” “Views per day,” etc.
public class PropertyView : BaseEntity
{
    public Guid PropertyId { get; set; }
    public Property Property { get; set; } = null!;
    
    public string? SessionId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime ViewedAt { get; set; }
}
