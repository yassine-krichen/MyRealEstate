using MediatR;

namespace MyRealEstate.Application.Commands.Content;

/// <summary>
/// Command to delete a content entry
/// </summary>
public class DeleteContentEntryCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}
