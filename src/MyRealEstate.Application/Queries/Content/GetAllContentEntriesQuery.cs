using MediatR;

namespace MyRealEstate.Application.Queries.Content;

/// <summary>
/// Query to get all content entries (for admin panel)
/// </summary>
public class GetAllContentEntriesQuery : IRequest<List<ContentEntryDto>>
{
}

public class ContentEntryDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string HtmlValue { get; set; } = string.Empty;
    public string? UpdatedByUserName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
