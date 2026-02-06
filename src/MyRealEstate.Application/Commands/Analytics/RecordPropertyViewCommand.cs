using MediatR;

namespace MyRealEstate.Application.Commands.Analytics;

/// <summary>
/// Command to record a property view for analytics
/// </summary>
public class RecordPropertyViewCommand : IRequest<Unit>
{
    public Guid PropertyId { get; set; }
    public string? SessionId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
