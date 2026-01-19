using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyRealEstate.Application.Interfaces;
using MyRealEstate.Domain.Entities;
using MyRealEstate.Domain.Enums;

namespace MyRealEstate.Application.Commands.Inquiries;

public record AssignInquiryCommand : IRequest<Unit>
{
    public Guid InquiryId { get; init; }
    public Guid AgentId { get; init; }
}

public class AssignInquiryCommandHandler : IRequestHandler<AssignInquiryCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AssignInquiryCommandHandler> _logger;

    public AssignInquiryCommandHandler(
        IApplicationDbContext context,
        ILogger<AssignInquiryCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Unit> Handle(AssignInquiryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling AssignInquiryCommand");

        var inquiry = await _context.Inquiries
            .FirstOrDefaultAsync(i => i.Id == request.InquiryId && !i.IsDeleted, cancellationToken);

        if (inquiry == null)
        {
            throw new InvalidOperationException($"Inquiry with ID {request.InquiryId} not found");
        }

        // Assign the agent (domain validates the agent exists via FK)
        inquiry.AssignToAgent(request.AgentId);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Handled AssignInquiryCommand successfully");
        return Unit.Value;
    }
}
