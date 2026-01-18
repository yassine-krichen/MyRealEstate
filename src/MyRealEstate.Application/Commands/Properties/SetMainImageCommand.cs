using MediatR;
using Microsoft.EntityFrameworkCore;
using MyRealEstate.Application.Common.Exceptions;
using MyRealEstate.Application.Common.Models;
using MyRealEstate.Application.Interfaces;
using MyRealEstate.Domain.Entities;

namespace MyRealEstate.Application.Commands.Properties;

public record SetMainImageCommand(Guid ImageId) : IRequest<Result>;

public class SetMainImageCommandHandler : IRequestHandler<SetMainImageCommand, Result>
{
    private readonly IApplicationDbContext _context;
    
    public SetMainImageCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Result> Handle(SetMainImageCommand request, CancellationToken cancellationToken)
    {
        var image = await _context.PropertyImages
            .Include(i => i.Property)
            .ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(i => i.Id == request.ImageId, cancellationToken);
        
        if (image == null)
        {
            throw new NotFoundException(nameof(PropertyImage), request.ImageId);
        }
        
        foreach (var img in image.Property.Images)
        {
            img.IsMain = img.Id == request.ImageId;
        }
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
