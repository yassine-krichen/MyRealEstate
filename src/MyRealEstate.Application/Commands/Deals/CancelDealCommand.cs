using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyRealEstate.Application.Interfaces;
using MyRealEstate.Domain.Enums;

namespace MyRealEstate.Application.Commands.Deals;

public record CancelDealCommand : IRequest<Unit>
{
    public Guid Id { get; init; }
    public string? CancellationReason { get; init; }
}

public class CancelDealCommandHandler : IRequestHandler<CancelDealCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CancelDealCommandHandler> _logger;

    public CancelDealCommandHandler(
        IApplicationDbContext context,
        ILogger<CancelDealCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Unit> Handle(CancelDealCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cancelling deal {DealId}", request.Id);

        var deal = await _context.Deals
            .Include(d => d.Property)
            .Include(d => d.Inquiry)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (deal == null)
        {
            throw new InvalidOperationException($"Deal with ID {request.Id} not found");
        }

        if (deal.Status == DealStatus.Completed)
        {
            throw new InvalidOperationException("Completed deals cannot be cancelled");
        }

        // Append cancellation reason to notes
        if (!string.IsNullOrWhiteSpace(request.CancellationReason))
        {
            deal.Notes = string.IsNullOrWhiteSpace(deal.Notes)
                ? $"Cancellation reason: {request.CancellationReason}"
                : $"{deal.Notes}\n\n--- Cancellation Reason ---\n{request.CancellationReason}";
        }

        // Cancel the deal using domain method
        deal.Cancel();

        // Revert property status to Published (was Available/Published before sale)
        deal.Property.Status = PropertyStatus.Published;
        deal.Property.ClosedDealId = null;
        deal.Property.UpdatedAt = DateTime.UtcNow;

        // Revert inquiry status if linked
        if (deal.Inquiry != null && deal.Inquiry.Status == InquiryStatus.Closed)
        {
            deal.Inquiry.Reopen();
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deal {DealId} cancelled successfully. Property {PropertyId} reverted to Published",
            deal.Id, deal.PropertyId);
        return Unit.Value;
    }
}
