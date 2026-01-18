using MediatR;
using Microsoft.EntityFrameworkCore;
using MyRealEstate.Application.Common.Exceptions;
using MyRealEstate.Application.Common.Models;
using MyRealEstate.Application.Interfaces;
using MyRealEstate.Domain.Entities;

namespace MyRealEstate.Application.Commands.Properties;

public record DeletePropertyCommand(Guid Id) : IRequest<Result>;

public class DeletePropertyCommandHandler : IRequestHandler<DeletePropertyCommand, Result>
{
    private readonly IApplicationDbContext _context;
    
    public DeletePropertyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Result> Handle(DeletePropertyCommand request, CancellationToken cancellationToken)
    {
        var property = await _context.Properties
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        
        if (property == null)
        {
            throw new NotFoundException(nameof(Property), request.Id);
        }
        
        property.SoftDelete();
        await _context.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
