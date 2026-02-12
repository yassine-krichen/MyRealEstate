using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyRealEstate.Application.DTOs;
using MyRealEstate.Application.Interfaces;
using MyRealEstate.Domain.Enums;

namespace MyRealEstate.Application.Queries.Deals;

public record GetDealStatisticsQuery : IRequest<DealStatisticsDto>
{
    public Guid? AgentId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}

public class GetDealStatisticsQueryHandler : IRequestHandler<GetDealStatisticsQuery, DealStatisticsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetDealStatisticsQueryHandler> _logger;

    public GetDealStatisticsQueryHandler(
        IApplicationDbContext context,
        ILogger<GetDealStatisticsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DealStatisticsDto> Handle(GetDealStatisticsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetDealStatisticsQuery");

        var query = _context.Deals.AsNoTracking().AsQueryable();

        if (request.AgentId.HasValue)
        {
            query = query.Where(d => d.AgentId == request.AgentId.Value);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(d => d.CreatedAt >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(d => d.CreatedAt <= request.ToDate.Value);
        }

        // Project only financial columns to memory (SQLite doesn't support SUM/AVG on decimal)
        var completedFinancials = await query
            .Where(d => d.Status == DealStatus.Completed)
            .Select(d => new { d.SalePrice, d.CommissionAmount })
            .ToListAsync(cancellationToken);

        var dto = new DealStatisticsDto
        {
            TotalDeals = await query.CountAsync(cancellationToken),
            CompletedDeals = completedFinancials.Count,
            PendingDeals = await query.CountAsync(d => d.Status == DealStatus.Pending, cancellationToken),
            CancelledDeals = await query.CountAsync(d => d.Status == DealStatus.Cancelled, cancellationToken),
            TotalRevenue = completedFinancials.Sum(d => d.SalePrice),
            TotalCommission = completedFinancials.Sum(d => d.CommissionAmount ?? 0),
            AverageSalePrice = completedFinancials.Any()
                ? completedFinancials.Average(d => d.SalePrice)
                : 0,
            AverageCommission = completedFinancials.Any(d => d.CommissionAmount.HasValue)
                ? completedFinancials.Where(d => d.CommissionAmount.HasValue).Average(d => d.CommissionAmount!.Value)
                : 0
        };

        _logger.LogInformation("Handled GetDealStatisticsQuery successfully");
        return dto;
    }
}
