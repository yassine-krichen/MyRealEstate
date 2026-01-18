using MediatR;
using Microsoft.EntityFrameworkCore;
using MyRealEstate.Application.Common.Exceptions;
using MyRealEstate.Application.Common.Models;
using MyRealEstate.Application.Interfaces;
using MyRealEstate.Domain.Entities;

namespace MyRealEstate.Application.Commands.Properties;

public record UploadPropertyImageCommand : IRequest<Result<Guid>>
{
    public Guid PropertyId { get; init; }
    public Stream FileStream { get; init; } = Stream.Null;
    public string FileName { get; init; } = string.Empty;
    public bool SetAsMain { get; init; }
}

public class UploadPropertyImageCommandHandler : IRequestHandler<UploadPropertyImageCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorage _fileStorage;
    
    public UploadPropertyImageCommandHandler(IApplicationDbContext context, IFileStorage fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }
    
    public async Task<Result<Guid>> Handle(UploadPropertyImageCommand request, CancellationToken cancellationToken)
    {
        var property = await _context.Properties
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == request.PropertyId, cancellationToken);
        
        if (property == null)
        {
            throw new NotFoundException(nameof(Property), request.PropertyId);
        }
        
        var uploadResult = await _fileStorage.SaveFileAsync(request.FileStream, request.FileName, cancellationToken);
        
        var propertyImage = new PropertyImage
        {
            PropertyId = request.PropertyId,
            FilePath = uploadResult.FilePath,
            FileName = uploadResult.FileName,
            FileSize = uploadResult.FileSize,
            IsMain = request.SetAsMain || !property.Images.Any(),
            CreatedAt = DateTime.UtcNow
        };
        
        if (propertyImage.IsMain)
        {
            foreach (var img in property.Images)
            {
                img.IsMain = false;
            }
        }
        
        _context.PropertyImages.Add(propertyImage);
        await _context.SaveChangesAsync(cancellationToken);
        
        return Result.Success(propertyImage.Id);
    }
}
