using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyRealEstate.Application.DTOs;
using MyRealEstate.Application.Interfaces;
using MyRealEstate.Domain.Enums;

namespace MyRealEstate.Application.Queries.Inquiries;

public record GetInquiriesQuery : IRequest<InquiryListResult>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public InquiryStatus? Status { get; init; }
    public Guid? AssignedToId { get; init; }
    public Guid? PropertyId { get; init; }
    public string? SearchTerm { get; init; }
    /// <summary>
    /// When true and AssignedToId is set, also includes unassigned New inquiries.
    /// Used for agent filtering: show assigned to them OR unassigned new inquiries.
    /// </summary>
    public bool? IncludeUnassigned { get; init; }
}

public class InquiryListResult
{
    public List<InquiryDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public class GetInquiriesQueryHandler : IRequestHandler<GetInquiriesQuery, InquiryListResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetInquiriesQueryHandler> _logger;

    public GetInquiriesQueryHandler(
        IApplicationDbContext context,
        ILogger<GetInquiriesQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<InquiryListResult> Handle(GetInquiriesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetInquiriesQuery");

        var query = _context.Inquiries
            .Include(i => i.Property)
            .Include(i => i.AssignedAgent)
            .Include(i => i.Messages)
            .Where(i => !i.IsDeleted);

        // Apply filters
        if (request.Status.HasValue)
        {
            query = query.Where(i => i.Status == request.Status.Value);
        }

        // Special handling for agent filtering (assigned to them OR unassigned new)
        if (request.AssignedToId.HasValue)
        {
            if (request.IncludeUnassigned == true)
            {
                // Agent view: Show inquiries assigned to them OR unassigned New inquiries
                query = query.Where(i => 
                    i.AssignedAgentId == request.AssignedToId.Value ||
                    (i.AssignedAgentId == null && i.Status == InquiryStatus.New));
            }
            else
            {
                // Normal filter: Show only inquiries assigned to specific agent
                query = query.Where(i => i.AssignedAgentId == request.AssignedToId.Value);
            }
        }

        if (request.PropertyId.HasValue)
        {
            query = query.Where(i => i.PropertyId == request.PropertyId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            query = query.Where(i =>
                i.VisitorName.ToLower().Contains(searchLower) ||
                i.VisitorEmail.ToLower().Contains(searchLower) ||
                i.InitialMessage.ToLower().Contains(searchLower) ||
                (i.Property != null && i.Property.Title.ToLower().Contains(searchLower)));
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var inquiries = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new InquiryDto
            {
                Id = i.Id,
                PropertyId = i.PropertyId ?? Guid.Empty,
                PropertyTitle = i.Property != null ? i.Property.Title : "General Inquiry",
                PropertyCity = i.Property != null && i.Property.Address != null ? i.Property.Address.City : null,
                ClientName = i.VisitorName,
                ClientEmail = i.VisitorEmail,
                ClientPhone = i.VisitorPhone,
                Message = i.InitialMessage,
                Status = i.Status,
                AssignedToId = i.AssignedAgentId,
                AssignedToName = i.AssignedAgent != null ? i.AssignedAgent.FullName : null,
                CreatedAt = i.CreatedAt,
                RespondedAt = i.Messages != null && i.Messages.Any() ? i.Messages.Min(m => m.CreatedAt) : (DateTime?)null,
                MessageCount = i.Messages != null ? i.Messages.Count : 0
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Handled GetInquiriesQuery successfully");

        return new InquiryListResult
        {
            Items = inquiries,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
