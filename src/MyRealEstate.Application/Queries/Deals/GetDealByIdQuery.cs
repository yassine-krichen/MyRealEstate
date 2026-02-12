using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyRealEstate.Application.DTOs;
using MyRealEstate.Application.Interfaces;

namespace MyRealEstate.Application.Queries.Deals;

public record GetDealByIdQuery : IRequest<DealDetailDto?>
{
    public Guid Id { get; init; }
}

public class GetDealByIdQueryHandler : IRequestHandler<GetDealByIdQuery, DealDetailDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetDealByIdQueryHandler> _logger;

    public GetDealByIdQueryHandler(
        IApplicationDbContext context,
        ILogger<GetDealByIdQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DealDetailDto?> Handle(GetDealByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetDealByIdQuery for Deal {DealId}", request.Id);

        var deal = await _context.Deals
            .Include(d => d.Property)
                .ThenInclude(p => p.Images)
            .Include(d => d.Agent)
            .Include(d => d.Inquiry)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (deal == null)
        {
            return null;
        }

        var dto = new DealDetailDto
        {
            Id = deal.Id,
            PropertyId = deal.PropertyId,
            PropertyTitle = deal.Property?.Title ?? "Unknown",
            PropertyCity = deal.Property?.Address?.City,
            PropertyMainImageUrl = deal.Property?.Images?.FirstOrDefault(i => i.IsMain)?.FilePath
                ?? deal.Property?.Images?.FirstOrDefault()?.FilePath,
            AgentId = deal.AgentId,
            AgentName = deal.Agent?.FullName ?? "Unknown",
            AgentEmail = deal.Agent?.Email,
            AgentPhone = deal.Agent?.PhoneNumber,
            InquiryId = deal.InquiryId,
            InquiryVisitorName = deal.Inquiry?.VisitorName,
            BuyerName = deal.BuyerName,
            BuyerEmail = deal.BuyerEmail,
            BuyerPhone = deal.BuyerPhone,
            SalePrice = deal.SalePrice,
            CommissionPercent = deal.CommissionPercent,
            CommissionAmount = deal.CommissionAmount,
            Status = deal.Status,
            Notes = deal.Notes,
            CreatedAt = deal.CreatedAt,
            UpdatedAt = deal.UpdatedAt,
            ClosedAt = deal.ClosedAt
        };

        _logger.LogInformation("Handled GetDealByIdQuery successfully");
        return dto;
    }
}
