using MediatR;
using Microsoft.EntityFrameworkCore;
using MyRealEstate.Application.Common.Models;
using MyRealEstate.Application.DTOs;
using MyRealEstate.Application.Interfaces;
using MyRealEstate.Domain.Enums;

namespace MyRealEstate.Application.Queries.Properties;

public record GetPropertiesQuery : IRequest<Result<PaginatedList<PropertyListDto>>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public PropertyStatus? Status { get; init; }
    public Guid? AgentId { get; init; }
    public string? PropertyType { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public int? MinBedrooms { get; init; }
    public string? City { get; init; }
}

public class GetPropertiesQueryHandler : IRequestHandler<GetPropertiesQuery, Result<PaginatedList<PropertyListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorage _fileStorage;
    
    public GetPropertiesQueryHandler(IApplicationDbContext context, IFileStorage fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }
    
    public async Task<Result<PaginatedList<PropertyListDto>>> Handle(GetPropertiesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Properties
            .Where(p => !p.IsDeleted)
            .AsQueryable();
        
        // Apply filters
        if (request.Status.HasValue)
        {
            query = query.Where(p => p.Status == request.Status.Value);
        }
        
        if (request.AgentId.HasValue)
        {
            query = query.Where(p => p.AgentId == request.AgentId.Value);
        }
        
        if (!string.IsNullOrWhiteSpace(request.PropertyType))
        {
            query = query.Where(p => p.PropertyType == request.PropertyType);
        }
        
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
        
        if (!string.IsNullOrWhiteSpace(request.City))
        {
            query = query.Where(p => p.Address.City.Contains(request.City));
        }
        
        // Order by newest first
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
