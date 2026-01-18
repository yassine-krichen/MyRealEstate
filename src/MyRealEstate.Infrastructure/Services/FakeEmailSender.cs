using Microsoft.Extensions.Logging;
using MyRealEstate.Application.Interfaces;

namespace MyRealEstate.Infrastructure.Services;

public class FakeEmailSender : IEmailSender
{
    private readonly ILogger<FakeEmailSender> _logger;
    
    public FakeEmailSender(ILogger<FakeEmailSender> logger)
    {
        _logger = logger;
    }
    
    public Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        return SendEmailAsync(to, subject, body, false, cancellationToken);
    }
    
    public Task SendEmailAsync(string to, string subject, string body, bool isHtml, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Email would be sent to {To} - Subject: {Subject}", to, subject);
        _logger.LogDebug("Email body: {Body}", body);
        
        return Task.CompletedTask;
    }
}
