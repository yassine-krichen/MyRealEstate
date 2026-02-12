using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyRealEstate.Application.DTOs;
using MyRealEstate.Application.Interfaces;
using MyRealEstate.Domain.Enums;

namespace MyRealEstate.Application.Queries.Deals;

public record GetAllDealsQuery : IRequest<DealListResult>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public DealStatus? Status { get; init; }
    public Guid? AgentId { get; init; }
    public string? SearchTerm { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}

public class DealListResult
{
    public List<DealListDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public class GetAllDealsQueryHandler : IRequestHandler<GetAllDealsQuery, DealListResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetAllDealsQueryHandler> _logger;

    public GetAllDealsQueryHandler(
        IApplicationDbContext context,
        ILogger<GetAllDealsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DealListResult> Handle(GetAllDealsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetAllDealsQuery");

        var query = _context.Deals
            .Include(d => d.Property)
            .Include(d => d.Agent)
            .AsQueryable();

        // Apply filters
        if (request.Status.HasValue)
        {
            query = query.Where(d => d.Status == request.Status.Value);
        }

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

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            query = query.Where(d =>
                (d.BuyerName != null && d.BuyerName.ToLower().Contains(searchLower)) ||
                d.Property.Title.ToLower().Contains(searchLower));
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var deals = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(d => new DealListDto
            {
                Id = d.Id,
                PropertyTitle = d.Property.Title,
                PropertyCity = d.Property.Address.City,
                BuyerName = d.BuyerName ?? "Unknown",
                BuyerEmail = d.BuyerEmail,
                AgentName = d.Agent.FullName,
                SalePrice = d.SalePrice,
                CommissionAmount = d.CommissionAmount,
                Status = d.Status,
                CreatedAt = d.CreatedAt,
                ClosedAt = d.ClosedAt
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Handled GetAllDealsQuery successfully");

        return new DealListResult
        {
            Items = deals,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
