using MediatR;
using Microsoft.EntityFrameworkCore;
using MyRealEstate.Application.Interfaces;

namespace MyRealEstate.Application.Queries.Content;

public class GetContentByKeyQueryHandler : IRequestHandler<GetContentByKeyQuery, string?>
{
    private readonly IApplicationDbContext _context;

    public GetContentByKeyQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string?> Handle(GetContentByKeyQuery request, CancellationToken cancellationToken)
    {
        var entry = await _context.ContentEntries
            .Where(c => c.Key == request.Key)
            .Select(c => c.HtmlValue)
            .FirstOrDefaultAsync(cancellationToken);

        return entry;
    }
}
