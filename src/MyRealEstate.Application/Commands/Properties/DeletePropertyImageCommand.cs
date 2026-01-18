using MediatR;
using Microsoft.EntityFrameworkCore;
using MyRealEstate.Application.Common.Exceptions;
using MyRealEstate.Application.Common.Models;
using MyRealEstate.Application.Interfaces;
using MyRealEstate.Domain.Entities;

namespace MyRealEstate.Application.Commands.Properties;

public record DeletePropertyImageCommand(Guid ImageId) : IRequest<Result>;

public class DeletePropertyImageCommandHandler : IRequestHandler<DeletePropertyImageCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorage _fileStorage;
    
    public DeletePropertyImageCommandHandler(IApplicationDbContext context, IFileStorage fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }
    
    public async Task<Result> Handle(DeletePropertyImageCommand request, CancellationToken cancellationToken)
    {
        var image = await _context.PropertyImages
            .Include(i => i.Property)
            .ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(i => i.Id == request.ImageId, cancellationToken);
        
        if (image == null)
        {
            throw new NotFoundException(nameof(PropertyImage), request.ImageId);
        }
        
        var wasMainImage = image.IsMain;
        var propertyId = image.PropertyId;
        
        await _fileStorage.DeleteFileAsync(image.FilePath, cancellationToken);
        
        _context.PropertyImages.Remove(image);
        await _context.SaveChangesAsync(cancellationToken);
        
        // If we deleted the main image, promote the first remaining image to main
        if (wasMainImage)
        {
            var remainingImages = await _context.PropertyImages
                .Where(i => i.PropertyId == propertyId)
                .ToListAsync(cancellationToken);
            
            if (remainingImages.Any())
            {
                remainingImages.First().IsMain = true;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        
        return Result.Success();
    }
}
