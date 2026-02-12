using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyRealEstate.Application.Interfaces;
using MyRealEstate.Domain.Enums;

namespace MyRealEstate.Application.Commands.Deals;

public record UpdateDealCommand : IRequest<Unit>
{
    public Guid Id { get; init; }
    public string BuyerName { get; init; } = string.Empty;
    public string BuyerEmail { get; init; } = string.Empty;
    public string? BuyerPhone { get; init; }
    public decimal SalePrice { get; init; }
    public decimal CommissionRate { get; init; } = 5.0m;
    public string? Notes { get; init; }
}

public class UpdateDealCommandHandler : IRequestHandler<UpdateDealCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateDealCommandHandler> _logger;

    public UpdateDealCommandHandler(
        IApplicationDbContext context,
        ILogger<UpdateDealCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdateDealCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating deal {DealId}", request.Id);

        var deal = await _context.Deals
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (deal == null)
        {
            throw new InvalidOperationException($"Deal with ID {request.Id} not found");
        }

        if (deal.Status != DealStatus.Pending)
        {
            throw new InvalidOperationException("Only pending deals can be updated");
        }

        deal.BuyerName = request.BuyerName;
        deal.BuyerEmail = request.BuyerEmail;
        deal.BuyerPhone = request.BuyerPhone;
        deal.SalePrice = request.SalePrice;
        deal.CommissionPercent = request.CommissionRate;
        deal.Notes = request.Notes;
        deal.UpdatedAt = DateTime.UtcNow;

        // Recalculate commission
        deal.CalculateCommission();

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deal {DealId} updated successfully", deal.Id);
        return Unit.Value;
    }
}
