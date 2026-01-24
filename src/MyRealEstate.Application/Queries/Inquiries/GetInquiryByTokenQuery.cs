using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyRealEstate.Application.DTOs;
using MyRealEstate.Application.Interfaces;
using MyRealEstate.Domain.Enums;

namespace MyRealEstate.Application.Queries.Inquiries;

/// <summary>
/// Query to retrieve an inquiry by its access token (for visitor tracking)
/// </summary>
public record GetInquiryByTokenQuery : IRequest<InquiryDetailDto?>
{
    public string Token { get; init; } = string.Empty;
}

public class GetInquiryByTokenQueryHandler : IRequestHandler<GetInquiryByTokenQuery, InquiryDetailDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetInquiryByTokenQueryHandler> _logger;

    public GetInquiryByTokenQueryHandler(
        IApplicationDbContext context,
        ILogger<GetInquiryByTokenQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<InquiryDetailDto?> Handle(GetInquiryByTokenQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetInquiryByTokenQuery for token");

        // Validate token format (should be 32 characters, URL-safe base64)
        if (string.IsNullOrWhiteSpace(request.Token) || request.Token.Length != 32)
        {
            _logger.LogWarning("Invalid token format provided");
            return null;
        }

        var inquiry = await _context.Inquiries
            .Include(i => i.Property)
            .Include(i => i.AssignedAgent)
            .Include(i => i.Messages.OrderBy(m => m.CreatedAt))
                .ThenInclude(m => m.SenderUser)
            .FirstOrDefaultAsync(i => i.AccessToken == request.Token && !i.IsDeleted, cancellationToken);

        if (inquiry == null)
        {
            _logger.LogWarning("Inquiry not found for provided token");
            return null;
        }

        var dto = new InquiryDetailDto
        {
            Id = inquiry.Id,
            PropertyId = inquiry.PropertyId ?? Guid.Empty,
            PropertyTitle = inquiry.Property?.Title ?? "General Inquiry",
            PropertyCity = inquiry.Property?.Address.City,
            ClientName = inquiry.VisitorName,
            ClientEmail = inquiry.VisitorEmail,
            ClientPhone = inquiry.VisitorPhone,
            Message = inquiry.InitialMessage,
            Status = inquiry.Status,
            AssignedToId = inquiry.AssignedAgentId,
            AssignedToName = inquiry.AssignedAgent?.FullName,
            CreatedAt = inquiry.CreatedAt,
            RespondedAt = inquiry.Messages.Any() ? inquiry.Messages.Min(m => m.CreatedAt) : null,
            Messages = inquiry.Messages.Select(m => new MessageDto
            {
                Id = m.Id,
                Message = m.Body,
                SenderType = m.SenderType,
                SenderName = m.SenderType == SenderType.Agent
                    ? m.SenderUser?.FullName ?? "Agent"
                    : inquiry.VisitorName,
                SentAt = m.CreatedAt
            }).ToList()
        };

        _logger.LogInformation("Handled GetInquiryByTokenQuery successfully");
        return dto;
    }
}
