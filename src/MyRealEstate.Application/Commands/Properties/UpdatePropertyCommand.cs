using MediatR;
using Microsoft.EntityFrameworkCore;
using MyRealEstate.Application.Common.Exceptions;
using MyRealEstate.Application.Common.Models;
using MyRealEstate.Application.Interfaces;
using MyRealEstate.Domain.Entities;
using MyRealEstate.Domain.ValueObjects;

namespace MyRealEstate.Application.Commands.Properties;

public record UpdatePropertyCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Currency { get; init; } = "TND";
    public string PropertyType { get; init; } = string.Empty;
    public int Bedrooms { get; init; }
    public int Bathrooms { get; init; }
    public decimal AreaSqM { get; init; }
    
    // Address fields
    public string AddressLine1 { get; init; } = string.Empty;
    public string? AddressLine2 { get; init; }
    public string City { get; init; } = string.Empty;
    public string? State { get; init; }
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = "Tunisia";
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
}

public class UpdatePropertyCommandHandler : IRequestHandler<UpdatePropertyCommand, Result>
{
    private readonly IApplicationDbContext _context;
    
    public UpdatePropertyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Result> Handle(UpdatePropertyCommand request, CancellationToken cancellationToken)
    {
        var property = await _context.Properties
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        
        if (property == null)
        {
            throw new NotFoundException(nameof(Property), request.Id);
        }
        
        property.Title = request.Title;
        property.Description = request.Description;
        property.Price = new Money(request.Price, request.Currency);
        property.PropertyType = request.PropertyType;
        property.Bedrooms = request.Bedrooms;
        property.Bathrooms = request.Bathrooms;
        property.AreaSqM = request.AreaSqM;
        property.Address = new Address(
            line1: request.AddressLine1,
            city: request.City,
            country: request.Country,
            line2: request.AddressLine2,
            state: request.State,
            postalCode: request.PostalCode,
            latitude: request.Latitude,
            longitude: request.Longitude
        );
        property.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
