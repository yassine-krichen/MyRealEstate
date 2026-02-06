using MyRealEstate.Domain.Entities;

namespace MyRealEstate.Application.Interfaces;

/// <summary>
/// Repository interface for PropertyView analytics
/// </summary>
public interface IPropertyViewRepository
{
    Task AddPropertyViewAsync(PropertyView propertyView, CancellationToken cancellationToken = default);
    Task<bool> HasRecentViewAsync(Guid propertyId, string? sessionId, DateTime since, CancellationToken cancellationToken = default);
    Task<List<PropertyViewStats>> GetMostViewedPropertiesAsync(int topCount, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
}

public class PropertyViewStats
{
    public Guid PropertyId { get; set; }
    public string PropertyTitle { get; set; } = string.Empty;
    public string PropertyCity { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public DateTime? LastViewedAt { get; set; }
}
