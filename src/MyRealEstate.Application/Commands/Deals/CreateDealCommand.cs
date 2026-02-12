using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyRealEstate.Application.Interfaces;
using MyRealEstate.Domain.Entities;
using MyRealEstate.Domain.Enums;

namespace MyRealEstate.Application.Commands.Deals;

public record CreateDealCommand : IRequest<Guid>
{
    public Guid PropertyId { get; init; }
    public Guid? InquiryId { get; init; }
    public Guid AgentId { get; init; }
    public string BuyerName { get; init; } = string.Empty;
    public string BuyerEmail { get; init; } = string.Empty;
    public string? BuyerPhone { get; init; }
    public decimal SalePrice { get; init; }
    public decimal CommissionRate { get; init; } = 5.0m;
    public string? Notes { get; init; }
}

public class CreateDealCommandHandler : IRequestHandler<CreateDealCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateDealCommandHandler> _logger;

    public CreateDealCommandHandler(
        IApplicationDbContext context,
        ILogger<CreateDealCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateDealCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating deal for Property {PropertyId} by Agent {AgentId}",
            request.PropertyId, request.AgentId);

        // Validate property exists and is not already sold
        var property = await _context.Properties
            .FirstOrDefaultAsync(p => p.Id == request.PropertyId && !p.IsDeleted, cancellationToken);

        if (property == null)
        {
            throw new InvalidOperationException($"Property with ID {request.PropertyId} not found");
        }

        if (property.Status == PropertyStatus.Sold)
        {
            throw new InvalidOperationException("Cannot create deal for an already sold property");
        }

        // Validate inquiry if provided
        Inquiry? inquiry = null;
        if (request.InquiryId.HasValue)
        {
            inquiry = await _context.Inquiries
                .FirstOrDefaultAsync(i => i.Id == request.InquiryId.Value && !i.IsDeleted, cancellationToken);

            if (inquiry == null)
            {
                throw new InvalidOperationException($"Inquiry with ID {request.InquiryId.Value} not found");
            }

            if (inquiry.Status == InquiryStatus.Closed)
            {
                throw new InvalidOperationException("Cannot create deal from a closed inquiry");
            }
        }

        // Create the deal
        var deal = new Deal
        {
            Id = Guid.NewGuid(),
            PropertyId = request.PropertyId,
            InquiryId = request.InquiryId,
            AgentId = request.AgentId,
            BuyerName = request.BuyerName,
            BuyerEmail = request.BuyerEmail,
            BuyerPhone = request.BuyerPhone,
            SalePrice = request.SalePrice,
            CommissionPercent = request.CommissionRate,
            Notes = request.Notes,
            Status = DealStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        // Calculate commission
        deal.CalculateCommission();

        // Update property status to Sold
        property.MarkAsSold(deal.Id);

        // Update inquiry status to Closed if linked
        if (inquiry != null)
        {
            inquiry.Close(deal.Id);
        }

        _context.Deals.Add(deal);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deal {DealId} created successfully for Property {PropertyId} with sale price {SalePrice}",
            deal.Id, deal.PropertyId, deal.SalePrice);

        return deal.Id;
    }
}
