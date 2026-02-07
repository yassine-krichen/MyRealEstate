using MediatR;
using Microsoft.EntityFrameworkCore;
using MyRealEstate.Application.Interfaces;

namespace MyRealEstate.Application.Queries.Content;

public class GetAllContentEntriesQueryHandler : IRequestHandler<GetAllContentEntriesQuery, List<ContentEntryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllContentEntriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ContentEntryDto>> Handle(GetAllContentEntriesQuery request, CancellationToken cancellationToken)
    {
        var entries = await _context.ContentEntries
            .OrderBy(c => c.Key)
            .Select(c => new ContentEntryDto
            {
                Id = c.Id,
                Key = c.Key,
                HtmlValue = c.HtmlValue,
                UpdatedByUserName = c.UpdatedByUser != null ? c.UpdatedByUser.FullName : null,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return entries;
    }
}
