using MediatR;
using Microsoft.Extensions.Logging;
using MyRealEstate.Application.Interfaces;
using MyRealEstate.Domain.Entities;

namespace MyRealEstate.Application.Commands.Analytics;

public class RecordPropertyViewCommandHandler : IRequestHandler<RecordPropertyViewCommand, Unit>
{
    private readonly IPropertyViewRepository _propertyViewRepository;
    private readonly ILogger<RecordPropertyViewCommandHandler> _logger;

    public RecordPropertyViewCommandHandler(
        IPropertyViewRepository propertyViewRepository,
        ILogger<RecordPropertyViewCommandHandler> logger)
    {
        _propertyViewRepository = propertyViewRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(RecordPropertyViewCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if this is a duplicate view from same session in last 30 minutes
            var recentView = await _propertyViewRepository.HasRecentViewAsync(
                request.PropertyId,
                request.SessionId,
                DateTime.UtcNow.AddMinutes(-30),
                cancellationToken);

            if (recentView)
            {
                // Don't record duplicate views from same session
                return Unit.Value;
            }

            var propertyView = new PropertyView
            {
                Id = Guid.NewGuid(),
                PropertyId = request.PropertyId,
                SessionId = request.SessionId,
                IpAddress = request.IpAddress,
                UserAgent = request.UserAgent,
                ViewedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _propertyViewRepository.AddPropertyViewAsync(propertyView, cancellationToken);

            _logger.LogInformation("Recorded property view for PropertyId {PropertyId}", request.PropertyId);
        }
        catch (Exception ex)
        {
            // Log but don't fail the request if view tracking fails
            _logger.LogError(ex, "Failed to record property view for PropertyId {PropertyId}", request.PropertyId);
        }

        return Unit.Value;
    }
}
