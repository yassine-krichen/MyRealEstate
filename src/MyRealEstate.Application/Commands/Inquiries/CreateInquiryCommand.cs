using System.Security.Cryptography;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyRealEstate.Application.Interfaces;
using MyRealEstate.Domain.Entities;
using MyRealEstate.Domain.Enums;

namespace MyRealEstate.Application.Commands.Inquiries;

public record CreateInquiryResponse
{
    public Guid InquiryId { get; init; }
    public string AccessToken { get; init; } = string.Empty;
}

public record CreateInquiryCommand : IRequest<CreateInquiryResponse>
{
    public Guid? PropertyId { get; init; }
    public string ClientName { get; init; } = string.Empty;
    public string ClientEmail { get; init; } = string.Empty;
    public string? ClientPhone { get; init; }
    public string Message { get; init; } = string.Empty;
}

public class CreateInquiryCommandHandler : IRequestHandler<CreateInquiryCommand, CreateInquiryResponse>
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

    public async Task<CreateInquiryResponse> Handle(CreateInquiryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling CreateInquiryCommand");

        // Verify property exists if PropertyId is provided
        if (request.PropertyId.HasValue)
        {
            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == request.PropertyId.Value && !p.IsDeleted, cancellationToken);

            if (property == null)
            {
                throw new InvalidOperationException($"Property with ID {request.PropertyId.Value} not found");
            }
        }

        // Generate cryptographically secure access token
        var accessToken = GenerateAccessToken();

        var inquiry = new Inquiry
        {
            PropertyId = request.PropertyId, // Can be null for general inquiries
            VisitorName = request.ClientName,
            VisitorEmail = request.ClientEmail,
            VisitorPhone = request.ClientPhone,
            InitialMessage = request.Message,
            AccessToken = accessToken,
            Status = InquiryStatus.New
        };

        _context.Inquiries.Add(inquiry);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Handled CreateInquiryCommand successfully. Access token generated.");
        
        return new CreateInquiryResponse
        {
            InquiryId = inquiry.Id,
            AccessToken = accessToken
        };
    }

    /// <summary>
    /// Generates a cryptographically secure 32-character access token
    /// </summary>
    private static string GenerateAccessToken()
    {
        // Generate 24 random bytes (will become 32 chars in base64 URL-safe encoding)
        var randomBytes = new byte[24];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        // Convert to base64 URL-safe string (replace +/= with -_)
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}
