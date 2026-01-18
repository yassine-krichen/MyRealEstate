using MediatR;
using Microsoft.EntityFrameworkCore;
using MyRealEstate.Application.Common.Exceptions;
using MyRealEstate.Application.Common.Models;
using MyRealEstate.Application.DTOs;
using MyRealEstate.Application.Interfaces;
using MyRealEstate.Domain.Entities;

namespace MyRealEstate.Application.Queries.Properties;

public record GetPropertyByIdQuery(Guid Id, bool TrackView = false) : IRequest<Result<PropertyDetailDto>>;

public class GetPropertyByIdQueryHandler : IRequestHandler<GetPropertyByIdQuery, Result<PropertyDetailDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorage _fileStorage;
    private readonly ICurrentUserService _currentUser;
    
    public GetPropertyByIdQueryHandler(IApplicationDbContext context, IFileStorage fileStorage, ICurrentUserService currentUser)
    {
        _context = context;
        _fileStorage = fileStorage;
        _currentUser = currentUser;
    }
    
    public async Task<Result<PropertyDetailDto>> Handle(GetPropertyByIdQuery request, CancellationToken cancellationToken)
    {
        var property = await _context.Properties
            .Include(p => p.Images)
            .Include(p => p.Agent)
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken);
        
        if (property == null)
        {
            throw new NotFoundException(nameof(Property), request.Id);
        }
        
        // Track view if requested
        if (request.TrackView)
        {
            var view = new PropertyView
            {
                PropertyId = property.Id,
                SessionId = _currentUser.UserId?.ToString(),
                ViewedAt = DateTime.UtcNow,
                IpAddress = null // Could be set from HttpContext if needed
            };
            _context.PropertyViews.Add(view);
            await _context.SaveChangesAsync(cancellationToken);
        }
        
        var viewCount = await _context.PropertyViews
            .CountAsync(v => v.PropertyId == property.Id, cancellationToken);
        
        var dto = new PropertyDetailDto
        {
            Id = property.Id,
            Title = property.Title,
            Description = property.Description,
            Price = property.Price.Amount,
            Currency = property.Price.Currency,
            PropertyType = property.PropertyType,
            Bedrooms = property.Bedrooms,
            Bathrooms = property.Bathrooms,
            AreaSqM = property.AreaSqM,
            Status = property.Status.ToString(),
            Address = new AddressDto
            {
                Line1 = property.Address.Line1,
                Line2 = property.Address.Line2,
                City = property.Address.City,
                State = property.Address.State,
                PostalCode = property.Address.PostalCode,
                Country = property.Address.Country,
                Latitude = property.Address.Latitude,
                Longitude = property.Address.Longitude
            },
            Agent = property.Agent != null ? new AgentDto
            {
                Id = property.Agent.Id,
                FullName = property.Agent.FullName,
                Email = property.Agent.Email ?? string.Empty,
                PhoneNumber = property.Agent.PhoneNumber
            } : null,
            Images = property.Images.Select(img => new PropertyImageDto
            {
                Id = img.Id,
                Url = _fileStorage.GetFileUrl(img.FilePath),
                FileName = img.FileName,
                IsMain = img.IsMain
            }).OrderByDescending(i => i.IsMain).ToList(),
            ViewCount = viewCount,
            CreatedAt = property.CreatedAt,
            UpdatedAt = property.UpdatedAt
        };
        
        return Result.Success(dto);
    }
}
