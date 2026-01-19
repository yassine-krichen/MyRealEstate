using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyRealEstate.Application.Interfaces;
using MyRealEstate.Domain.Enums;

namespace MyRealEstate.Application.Commands.Inquiries;

public record UpdateInquiryStatusCommand : IRequest<Unit>
{
    public Guid InquiryId { get; init; }
    public InquiryStatus Status { get; init; }
}

public class UpdateInquiryStatusCommandHandler : IRequestHandler<UpdateInquiryStatusCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateInquiryStatusCommandHandler> _logger;

    public UpdateInquiryStatusCommandHandler(
        IApplicationDbContext context,
        ILogger<UpdateInquiryStatusCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdateInquiryStatusCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling UpdateInquiryStatusCommand");

        var inquiry = await _context.Inquiries
            .FirstOrDefaultAsync(i => i.Id == request.InquiryId && !i.IsDeleted, cancellationToken);

        if (inquiry == null)
        {
            throw new InvalidOperationException($"Inquiry with ID {request.InquiryId} not found");
        }

        // Use domain methods where appropriate, or set status directly for manual updates
        switch (request.Status)
        {
            case InquiryStatus.InProgress:
                // Try to use domain method, but if it fails (wrong status), set directly
                try
                {
                    inquiry.StartProgress();
                }
                catch (InvalidOperationException)
                {
                    // Allow manual override
                    inquiry.Status = InquiryStatus.InProgress;
                    inquiry.UpdatedAt = DateTime.UtcNow;
                }
                break;
                
            case InquiryStatus.Answered:
                try
                {
                    inquiry.MarkAsAnswered();
                }
                catch (InvalidOperationException)
                {
                    // Allow manual override
                    inquiry.Status = InquiryStatus.Answered;
                    inquiry.UpdatedAt = DateTime.UtcNow;
                }
                break;
                
            case InquiryStatus.Closed:
                inquiry.Close();
                break;
                
            default:
                // For New, Assigned - set directly
                inquiry.Status = request.Status;
                inquiry.UpdatedAt = DateTime.UtcNow;
                break;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Handled UpdateInquiryStatusCommand successfully");
        return Unit.Value;
    }
}
