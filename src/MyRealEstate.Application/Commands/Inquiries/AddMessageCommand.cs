using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyRealEstate.Application.Interfaces;
using MyRealEstate.Domain.Entities;
using MyRealEstate.Domain.Enums;

namespace MyRealEstate.Application.Commands.Inquiries;

public record AddMessageCommand : IRequest<Guid>
{
    public Guid InquiryId { get; init; }
    public string Message { get; init; } = string.Empty;
    public SenderType SenderType { get; init; }
    public Guid? SenderId { get; init; }
}

public class AddMessageCommandHandler : IRequestHandler<AddMessageCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AddMessageCommandHandler> _logger;

    public AddMessageCommandHandler(
        IApplicationDbContext context,
        ILogger<AddMessageCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Guid> Handle(AddMessageCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling AddMessageCommand");

        var inquiry = await _context.Inquiries
            .FirstOrDefaultAsync(i => i.Id == request.InquiryId && !i.IsDeleted, cancellationToken);

        if (inquiry == null)
        {
            throw new InvalidOperationException($"Inquiry with ID {request.InquiryId} not found");
        }

        var message = new ConversationMessage
        {
            InquiryId = request.InquiryId,
            Body = request.Message,
            SenderType = request.SenderType,
            SenderUserId = request.SenderId,
            IsInternalNote = false
        };

        _context.ConversationMessages.Add(message);

        // Auto-status progression when agent replies
        if (request.SenderType == SenderType.Agent || request.SenderType == SenderType.Admin)
        {
            if (inquiry.Status == InquiryStatus.New && request.SenderId.HasValue)
            {
                // New inquiry + agent replies = assign and move to assigned
                inquiry.AssignToAgent(request.SenderId.Value);
            }
            else if (inquiry.Status == InquiryStatus.Assigned)
            {
                // Assigned + agent replies = move to in progress
                inquiry.StartProgress();
            }
            // If already InProgress, Answered, or Closed - status doesn't change automatically
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Handled AddMessageCommand successfully");
        return message.Id;
    }
}
