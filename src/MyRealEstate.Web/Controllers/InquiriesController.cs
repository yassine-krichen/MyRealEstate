using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyRealEstate.Application.Commands.Inquiries;
using MyRealEstate.Application.Queries.Inquiries;
using MyRealEstate.Domain.Enums;
using MyRealEstate.Web.Models;

namespace MyRealEstate.Web.Controllers;

public class InquiriesController : Controller
{
    private readonly IMediator _mediator;
    private readonly ILogger<InquiriesController> _logger;

    public InquiriesController(IMediator mediator, ILogger<InquiriesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    // POST: /Inquiries/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateInquiryViewModel model)
    {
        // Log received data for debugging
        _logger.LogInformation("Received inquiry: PropertyId={PropertyId}, Name={Name}, Email={Email}, Phone={Phone}, MessageLength={MessageLength}",
            model.PropertyId, model.VisitorName, model.VisitorEmail, model.VisitorPhone, model.Message?.Length ?? 0);
        
        if (!ModelState.IsValid)
        {
            // Log all validation errors for debugging
            _logger.LogWarning("ModelState is invalid. Errors:");
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                if (state?.Errors.Count > 0)
                {
                    foreach (var error in state.Errors)
                    {
                        _logger.LogWarning("Field: {Field}, Error: {Error}", key, error.ErrorMessage);
                    }
                }
            }
            
            TempData["Error"] = "Please fill in all required fields.";
            
            if (model.PropertyId.HasValue)
            {
                return RedirectToAction("Details", "Properties", new { id = model.PropertyId.Value });
            }
            return RedirectToAction("Index", "Home");
        }

        try
        {
            var command = new CreateInquiryCommand
            {
                PropertyId = model.PropertyId,
                ClientName = model.VisitorName,
                ClientEmail = model.VisitorEmail,
                ClientPhone = model.VisitorPhone,
                Message = model.Message
            };

            var response = await _mediator.Send(command);

            var trackingUrl = Url.Action("Track", "Inquiries", new { token = response.AccessToken }, Request.Scheme);

            var viewModel = new InquiryCreatedViewModel
            {
                Token = response.AccessToken,
                TrackingUrl = trackingUrl!,
                VisitorEmail = model.VisitorEmail
            };

            return View("Created", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create inquiry");
            TempData["Error"] = "Failed to submit inquiry. Please try again.";
            
            if (model.PropertyId.HasValue)
            {
                return RedirectToAction("Details", "Properties", new { id = model.PropertyId.Value });
            }
            return RedirectToAction("Index", "Home");
        }
    }

    // GET: /Inquiries/Track?token=ABC123...
    public async Task<IActionResult> Track(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return View("InvalidToken");
        }

        var query = new GetInquiryByTokenQuery { Token = token };
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return View("InvalidToken");
        }

        var viewModel = new InquiryTrackingViewModel
        {
            Inquiry = result,
            Token = token,
            ReplyForm = new AddMessageViewModel { Token = token }
        };

        return View(viewModel);
    }

    // POST: /Inquiries/AddMessage
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMessage(AddMessageViewModel model)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.Message))
        {
            TempData["Error"] = "Message cannot be empty.";
            return RedirectToAction("Track", new { token = model.Token });
        }

        // First verify the token exists
        var inquiryQuery = new GetInquiryByTokenQuery { Token = model.Token };
        var inquiry = await _mediator.Send(inquiryQuery);

        if (inquiry == null)
        {
            return View("InvalidToken");
        }

        // Check if inquiry is closed
        if (inquiry.Status == InquiryStatus.Closed)
        {
            TempData["Error"] = "This inquiry is closed and cannot receive new messages.";
            return RedirectToAction("Track", new { token = model.Token });
        }

        try
        {
            var command = new AddMessageCommand
            {
                InquiryId = inquiry.Id,
                Message = model.Message,
                SenderType = SenderType.Visitor,
                SenderId = null
            };

            await _mediator.Send(command);
            TempData["Success"] = "Your message has been sent successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add message to inquiry {InquiryId}", inquiry.Id);
            TempData["Error"] = "Failed to send message. Please try again.";
        }

        return RedirectToAction("Track", new { token = model.Token });
    }

    // POST: /Inquiries/MarkAnswered
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAnswered(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return View("InvalidToken");
        }

        // Get inquiry to get its ID
        var inquiryQuery = new GetInquiryByTokenQuery { Token = token };
        var inquiry = await _mediator.Send(inquiryQuery);

        if (inquiry == null)
        {
            return View("InvalidToken");
        }

        try
        {
            var command = new UpdateInquiryStatusCommand
            {
                InquiryId = inquiry.Id,
                Status = InquiryStatus.Answered
            };

            await _mediator.Send(command);
            TempData["Success"] = "Thank you! We've marked your inquiry as answered.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark inquiry {InquiryId} as answered", inquiry.Id);
            TempData["Error"] = "Failed to update status. Please try again.";
        }

        return RedirectToAction("Track", new { token });
    }

    // POST: /Inquiries/Close
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return View("InvalidToken");
        }

        // Get inquiry to get its ID
        var inquiryQuery = new GetInquiryByTokenQuery { Token = token };
        var inquiry = await _mediator.Send(inquiryQuery);

        if (inquiry == null)
        {
            return View("InvalidToken");
        }

        try
        {
            var command = new UpdateInquiryStatusCommand
            {
                InquiryId = inquiry.Id,
                Status = InquiryStatus.Closed
            };

            await _mediator.Send(command);
            TempData["Success"] = "Your inquiry has been closed.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to close inquiry {InquiryId}", inquiry.Id);
            TempData["Error"] = "Failed to close inquiry. Please try again.";
        }

        return RedirectToAction("Track", new { token });
    }
}
