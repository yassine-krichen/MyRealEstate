using MediatR;

namespace MyRealEstate.Application.Queries.Content;

/// <summary>
/// Query to get content by key (for displaying on site)
/// </summary>
public class GetContentByKeyQuery : IRequest<string?>
{
    public string Key { get; set; } = string.Empty;
    
    public GetContentByKeyQuery(string key)
    {
        Key = key;
    }
}
