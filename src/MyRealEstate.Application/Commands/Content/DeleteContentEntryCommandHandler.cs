using MediatR;
using Microsoft.EntityFrameworkCore;
using MyRealEstate.Application.Interfaces;

namespace MyRealEstate.Application.Commands.Content;

public class DeleteContentEntryCommandHandler : IRequestHandler<DeleteContentEntryCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteContentEntryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteContentEntryCommand request, CancellationToken cancellationToken)
    {
        var contentEntry = await _context.ContentEntries
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (contentEntry == null)
        {
            throw new KeyNotFoundException($"Content entry with ID '{request.Id}' not found.");
        }

        _context.ContentEntries.Remove(contentEntry);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
