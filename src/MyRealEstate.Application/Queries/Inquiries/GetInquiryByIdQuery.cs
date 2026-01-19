using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyRealEstate.Application.DTOs;
using MyRealEstate.Application.Interfaces;

namespace MyRealEstate.Application.Queries.Inquiries;

public record GetInquiryByIdQuery : IRequest<InquiryDetailDto?>
{
    public Guid Id { get; init; }
}

public class GetInquiryByIdQueryHandler : IRequestHandler<GetInquiryByIdQuery, InquiryDetailDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetInquiryByIdQueryHandler> _logger;

    public GetInquiryByIdQueryHandler(
        IApplicationDbContext context,
        ILogger<GetInquiryByIdQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<InquiryDetailDto?> Handle(GetInquiryByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetInquiryByIdQuery");

        var inquiry = await _context.Inquiries
            .Include(i => i.Property)
            .Include(i => i.AssignedAgent)
            .Include(i => i.Messages.OrderBy(m => m.CreatedAt))
                .ThenInclude(m => m.SenderUser)
            .Where(i => i.Id == request.Id && !i.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (inquiry == null)
        {
            return null;
        }

        var dto = new InquiryDetailDto
        {
            Id = inquiry.Id,
            PropertyId = inquiry.PropertyId ?? Guid.Empty,
            PropertyTitle = inquiry.Property?.Title ?? "Unknown",
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
            MessageCount = inquiry.Messages.Count,
            Messages = inquiry.Messages.Select(m => new MessageDto
            {
                Id = m.Id,
                InquiryId = m.InquiryId,
                SenderType = m.SenderType,
                SenderName = m.SenderUser?.FullName,
                Message = m.Body,
                SentAt = m.CreatedAt
            }).ToList()
        };

        _logger.LogInformation("Handled GetInquiryByIdQuery successfully");
        return dto;
    }
}
