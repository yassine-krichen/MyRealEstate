using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyRealEstate.Application.Interfaces;
using MyRealEstate.Domain.Entities;
using MyRealEstate.Domain.Enums;

namespace MyRealEstate.Application.Commands.Inquiries;

public record CreateInquiryCommand : IRequest<Guid>
{
    public Guid PropertyId { get; init; }
    public string ClientName { get; init; } = string.Empty;
    public string ClientEmail { get; init; } = string.Empty;
    public string? ClientPhone { get; init; }
    public string Message { get; init; } = string.Empty;
}

public class CreateInquiryCommandHandler : IRequestHandler<CreateInquiryCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateInquiryCommandHandler> _logger;

    public CreateInquiryCommandHandler(
        IApplicationDbContext context,
        ILogger<CreateInquiryCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateInquiryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling CreateInquiryCommand");

        // Verify property exists
        var property = await _context.Properties
            .FirstOrDefaultAsync(p => p.Id == request.PropertyId && !p.IsDeleted, cancellationToken);

        if (property == null)
        {
            throw new InvalidOperationException($"Property with ID {request.PropertyId} not found");
        }

        var inquiry = new Inquiry
        {
            PropertyId = request.PropertyId,
            VisitorName = request.ClientName,
            VisitorEmail = request.ClientEmail,
            VisitorPhone = request.ClientPhone,
            InitialMessage = request.Message,
            Status = InquiryStatus.New
        };

        _context.Inquiries.Add(inquiry);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Handled CreateInquiryCommand successfully");
        return inquiry.Id;
    }
}
