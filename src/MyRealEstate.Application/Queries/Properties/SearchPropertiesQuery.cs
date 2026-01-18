using MediatR;
using Microsoft.EntityFrameworkCore;
using MyRealEstate.Application.Common.Models;
using MyRealEstate.Application.DTOs;
using MyRealEstate.Application.Interfaces;

namespace MyRealEstate.Application.Queries.Properties;

public record SearchPropertiesQuery : IRequest<Result<PaginatedList<PropertyListDto>>>
{
    public string? SearchTerm { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public int? MinBedrooms { get; init; }
    public int? MaxBedrooms { get; init; }
    public string? PropertyType { get; init; }
    public string? City { get; init; }
}

public class SearchPropertiesQueryHandler : IRequestHandler<SearchPropertiesQuery, Result<PaginatedList<PropertyListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorage _fileStorage;
    
    public SearchPropertiesQueryHandler(IApplicationDbContext context, IFileStorage fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }
    
    public async Task<Result<PaginatedList<PropertyListDto>>> Handle(SearchPropertiesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Properties
            .Where(p => !p.IsDeleted && p.Status == Domain.Enums.PropertyStatus.Published)
            .AsQueryable();
        
        // Text search in title and description
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(p => 
                p.Title.ToLower().Contains(searchTerm) || 
                p.Description.ToLower().Contains(searchTerm));
        }
        
        // Apply filters
        if (request.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price.Amount >= request.MinPrice.Value);
        }
        
        if (request.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price.Amount <= request.MaxPrice.Value);
        }
        
        if (request.MinBedrooms.HasValue)
        {
            query = query.Where(p => p.Bedrooms >= request.MinBedrooms.Value);
        }
        
        if (request.MaxBedrooms.HasValue)
        {
            query = query.Where(p => p.Bedrooms <= request.MaxBedrooms.Value);
        }
        
        if (!string.IsNullOrWhiteSpace(request.PropertyType))
        {
            query = query.Where(p => p.PropertyType == request.PropertyType);
        }
        
        if (!string.IsNullOrWhiteSpace(request.City))
        {
            query = query.Where(p => p.Address.City.ToLower().Contains(request.City.ToLower()));
        }
        
        // Order by relevance (newest first for now, could be improved with ranking)
        query = query.OrderByDescending(p => p.CreatedAt);
        
        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Include(p => p.Images)
            .Select(p => new PropertyListDto
            {
                Id = p.Id,
                Title = p.Title,
                Price = p.Price.Amount,
                Currency = p.Price.Currency,
                PropertyType = p.PropertyType,
                Bedrooms = p.Bedrooms,
                Bathrooms = p.Bathrooms,
                AreaSqM = p.AreaSqM,
                City = p.Address.City,
                Status = p.Status.ToString(),
                MainImageUrl = p.Images.Where(i => i.IsMain).Select(i => i.FilePath).FirstOrDefault(),
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(cancellationToken);
        
        // Convert file paths to URLs
        foreach (var item in items)
        {
            if (!string.IsNullOrEmpty(item.MainImageUrl))
            {
                item.MainImageUrl = _fileStorage.GetFileUrl(item.MainImageUrl);
            }
        }
        
        var result = new PaginatedList<PropertyListDto>(items, totalCount, request.Page, request.PageSize);
        
        return Result.Success(result);
    }
}
