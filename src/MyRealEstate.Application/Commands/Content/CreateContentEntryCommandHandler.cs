using MediatR;
using Microsoft.EntityFrameworkCore;
using MyRealEstate.Application.Interfaces;
using MyRealEstate.Domain.Entities;

namespace MyRealEstate.Application.Commands.Content;

public class CreateContentEntryCommandHandler : IRequestHandler<CreateContentEntryCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateContentEntryCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateContentEntryCommand request, CancellationToken cancellationToken)
    {
        // Check if key already exists
        var exists = await _context.ContentEntries
            .AnyAsync(c => c.Key == request.Key, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException($"Content entry with key '{request.Key}' already exists.");
        }

        var contentEntry = new ContentEntry
        {
            Id = Guid.NewGuid(),
            Key = request.Key,
            HtmlValue = request.HtmlValue,
            UpdatedByUserId = _currentUserService.UserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.ContentEntries.AddAsync(contentEntry, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return contentEntry.Id;
    }
}
