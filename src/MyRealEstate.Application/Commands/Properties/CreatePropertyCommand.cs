using MediatR;
using MyRealEstate.Application.Common.Models;
using MyRealEstate.Application.Interfaces;
using MyRealEstate.Domain.Entities;
using MyRealEstate.Domain.Enums;
using MyRealEstate.Domain.ValueObjects;

namespace MyRealEstate.Application.Commands.Properties;

public record CreatePropertyCommand : IRequest<Result<Guid>>
{
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
    
    public Guid? AgentId { get; init; }
}

public class CreatePropertyCommandHandler : IRequestHandler<CreatePropertyCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    
    public CreatePropertyCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }
    
    public async Task<Result<Guid>> Handle(CreatePropertyCommand request, CancellationToken cancellationToken)
    {
        var property = new Property
        {
            Title = request.Title,
            Description = request.Description,
            Price = new Money(request.Price, request.Currency),
            PropertyType = request.PropertyType,
            Status = PropertyStatus.Draft,
            Bedrooms = request.Bedrooms,
            Bathrooms = request.Bathrooms,
            AreaSqM = request.AreaSqM,
            Address = new Address(
                line1: request.AddressLine1,
                city: request.City,
                country: request.Country,
                line2: request.AddressLine2,
                state: request.State,
                postalCode: request.PostalCode,
                latitude: request.Latitude,
                longitude: request.Longitude
            ),
            AgentId = request.AgentId ?? _currentUser.UserId,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Properties.Add(property);
        await _context.SaveChangesAsync(cancellationToken);
        
        return Result.Success(property.Id);
    }
}
