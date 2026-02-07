using MediatR;
using Microsoft.EntityFrameworkCore;
using MyRealEstate.Application.Interfaces;

namespace MyRealEstate.Application.Commands.Content;

public class UpdateContentEntryCommandHandler : IRequestHandler<UpdateContentEntryCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateContentEntryCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(UpdateContentEntryCommand request, CancellationToken cancellationToken)
    {
        var contentEntry = await _context.ContentEntries
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (contentEntry == null)
        {
            throw new KeyNotFoundException($"Content entry with ID '{request.Id}' not found.");
        }

        // Check if key is being changed and if new key already exists
        if (contentEntry.Key != request.Key)
        {
            var keyExists = await _context.ContentEntries
                .AnyAsync(c => c.Key == request.Key && c.Id != request.Id, cancellationToken);

            if (keyExists)
            {
                throw new InvalidOperationException($"Content entry with key '{request.Key}' already exists.");
            }

            contentEntry.Key = request.Key;
        }

        contentEntry.HtmlValue = request.HtmlValue;
        contentEntry.UpdatedByUserId = _currentUserService.UserId;
        contentEntry.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
