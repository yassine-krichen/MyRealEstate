using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyRealEstate.Application.Interfaces;
using MyRealEstate.Domain.Enums;

namespace MyRealEstate.Application.Commands.Deals;

public record CompleteDealCommand : IRequest<Unit>
{
    public Guid Id { get; init; }
    public string? Notes { get; init; }
}

public class CompleteDealCommandHandler : IRequestHandler<CompleteDealCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CompleteDealCommandHandler> _logger;

    public CompleteDealCommandHandler(
        IApplicationDbContext context,
        ILogger<CompleteDealCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Unit> Handle(CompleteDealCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Completing deal {DealId}", request.Id);

        var deal = await _context.Deals
            .Include(d => d.Property)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (deal == null)
        {
            throw new InvalidOperationException($"Deal with ID {request.Id} not found");
        }

        if (deal.Status != DealStatus.Pending)
        {
            throw new InvalidOperationException("Only pending deals can be completed");
        }

        // Append notes if provided
        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            deal.Notes = string.IsNullOrWhiteSpace(deal.Notes)
                ? request.Notes
                : $"{deal.Notes}\n\n--- Completion Notes ---\n{request.Notes}";
        }

        // Complete the deal using domain method
        deal.Complete();

        // Ensure property status is Sold
        if (deal.Property.Status != PropertyStatus.Sold)
        {
            deal.Property.MarkAsSold(deal.Id);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deal {DealId} completed successfully", deal.Id);
        return Unit.Value;
    }
}
