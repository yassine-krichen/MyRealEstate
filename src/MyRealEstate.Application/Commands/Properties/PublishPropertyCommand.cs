using MediatR;
using Microsoft.EntityFrameworkCore;
using MyRealEstate.Application.Common.Exceptions;
using MyRealEstate.Application.Common.Models;
using MyRealEstate.Application.Interfaces;
using MyRealEstate.Domain.Entities;
using MyRealEstate.Domain.Enums;

namespace MyRealEstate.Application.Commands.Properties;

public record PublishPropertyCommand(Guid Id) : IRequest<Result>;

public class PublishPropertyCommandHandler : IRequestHandler<PublishPropertyCommand, Result>
{
    private readonly IApplicationDbContext _context;
    
    public PublishPropertyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Result> Handle(PublishPropertyCommand request, CancellationToken cancellationToken)
    {
        var property = await _context.Properties
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        
        if (property == null)
        {
            throw new NotFoundException(nameof(Property), request.Id);
        }
        
        if (property.Price.Amount <= 0)
        {
            return Result.Failure("Cannot publish property with price of 0 or less");
        }
        
        property.Publish();
        await _context.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
