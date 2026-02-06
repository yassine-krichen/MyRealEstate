using Microsoft.EntityFrameworkCore;
using MyRealEstate.Application.Interfaces;
using MyRealEstate.Domain.Entities;
using MyRealEstate.Infrastructure.Data;

namespace MyRealEstate.Infrastructure.Repositories;

public class PropertyViewRepository : IPropertyViewRepository
{
    private readonly ApplicationDbContext _context;

    public PropertyViewRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddPropertyViewAsync(PropertyView propertyView, CancellationToken cancellationToken = default)
    {
        await _context.PropertyViews.AddAsync(propertyView, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> HasRecentViewAsync(Guid propertyId, string? sessionId, DateTime since, CancellationToken cancellationToken = default)
    {
        return await _context.PropertyViews
            .Where(pv => pv.PropertyId == propertyId
                && pv.SessionId == sessionId
                && pv.ViewedAt > since)
            .AnyAsync(cancellationToken);
    }

    public async Task<List<PropertyViewStats>> GetMostViewedPropertiesAsync(int topCount, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.PropertyViews.AsQueryable();

        // Apply date filters
        if (fromDate.HasValue)
        {
            query = query.Where(pv => pv.ViewedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(pv => pv.ViewedAt <= toDate.Value);
        }

        var stats = await query
            .GroupBy(pv => new { pv.PropertyId, pv.Property.Title, pv.Property.Address.City })
            .Select(g => new PropertyViewStats
            {
                PropertyId = g.Key.PropertyId,
                PropertyTitle = g.Key.Title,
                PropertyCity = g.Key.City,
                ViewCount = g.Count(),
                LastViewedAt = g.Max(pv => pv.ViewedAt)
            })
            .OrderByDescending(x => x.ViewCount)
            .Take(topCount)
            .ToListAsync(cancellationToken);

        return stats;
    }
}
