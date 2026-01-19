using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyRealEstate.Application.Commands.Inquiries;
using MyRealEstate.Application.Queries.Inquiries;
using MyRealEstate.Domain.Entities;
using MyRealEstate.Domain.Enums;
using MyRealEstate.Web.Models;

namespace MyRealEstate.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Agent")]
public class InquiriesController : Controller
{
    private readonly IMediator _mediator;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<InquiriesController> _logger;

    public InquiriesController(
        IMediator mediator,
        UserManager<User> userManager,
        ILogger<InquiriesController> logger)
    {
        _mediator = mediator;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, InquiryStatus? status = null, Guid? assignedToId = null, string? searchTerm = null)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var isAdmin = User.IsInRole("Admin");
        var isAgent = User.IsInRole("Agent");

        // Apply role-based filtering
        Guid? filterAssignedToId = assignedToId;
        bool? includeUnassigned = null;

        if (isAgent && !isAdmin)
        {
            // Agents can only see:
            // 1. New unassigned inquiries (Status = New AND AssignedAgentId = null)
            // 2. Inquiries assigned to them
            filterAssignedToId = currentUser.Id;
            includeUnassigned = true; // Also include unassigned
        }
        // Admin sees everything (no additional filtering)

        var query = new GetInquiriesQuery
        {
            Page = page,
            PageSize = 20,
            Status = status,
            AssignedToId = filterAssignedToId,
            IncludeUnassigned = includeUnassigned,
            SearchTerm = searchTerm
        };

        var result = await _mediator.Send(query);

        var viewModel = new InquirySearchViewModel
        {
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            Status = status,
            AssignedToId = assignedToId,
            SearchTerm = searchTerm,
            Results = result.Items.Select(i => new InquiryListViewModel
            {
                Id = i.Id,
                PropertyTitle = i.PropertyTitle,
                PropertyCity = i.PropertyCity,
                ClientName = i.ClientName,
                ClientEmail = i.ClientEmail,
                Status = i.Status,
                AssignedToName = i.AssignedToName,
                CreatedAt = i.CreatedAt,
                MessageCount = i.MessageCount
            }).ToList()
        };

        // Get agents for filter dropdown (Admin only)
        if (isAdmin)
        {
            ViewBag.Agents = (await _userManager.GetUsersInRoleAsync("Agent"))
                .Where(u => u.IsActive)
                .Select(u => new { Id = u.Id, Name = u.FullName })
                .ToList();
        }

        ViewBag.IsAdmin = isAdmin;
        ViewBag.CurrentUserId = currentUser.Id;

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var query = new GetInquiryByIdQuery { Id = id };
        var inquiry = await _mediator.Send(query);

        if (inquiry == null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var isAdmin = User.IsInRole("Admin");
        var isAgent = User.IsInRole("Agent");

        // Authorization check: Agents can only view their own inquiries or unassigned new ones
        if (isAgent && !isAdmin)
        {
            var isAssignedToAgent = inquiry.AssignedToId == currentUser.Id;
            var isUnassignedNew = !inquiry.AssignedToId.HasValue && inquiry.Status == InquiryStatus.New;

            if (!isAssignedToAgent && !isUnassignedNew)
            {
                _logger.LogWarning("Agent {AgentId} attempted to access inquiry {InquiryId} they don't have access to", currentUser.Id, id);
                return Forbid(); // 403 Forbidden
            }
        }

        var viewModel = new InquiryDetailViewModel
        {
            Id = inquiry.Id,
            PropertyId = inquiry.PropertyId,
            PropertyTitle = inquiry.PropertyTitle,
            PropertyCity = inquiry.PropertyCity,
            ClientName = inquiry.ClientName,
            ClientEmail = inquiry.ClientEmail,
            ClientPhone = inquiry.ClientPhone,
            Message = inquiry.Message,
            Status = inquiry.Status,
            AssignedToId = inquiry.AssignedToId,
            AssignedToName = inquiry.AssignedToName,
            CreatedAt = inquiry.CreatedAt,
            RespondedAt = inquiry.RespondedAt,
            Messages = inquiry.Messages.Select(m => new MessageViewModel
            {
                Id = m.Id,
                SenderType = m.SenderType,
                SenderName = m.SenderName ?? (m.SenderType == SenderType.Visitor ? inquiry.ClientName : "Agent"),
                Message = m.Message,
                SentAt = m.SentAt
            }).ToList()
        };

        // Get agents for assignment dropdown (Admin only)
        if (isAdmin)
        {
            ViewBag.Agents = (await _userManager.GetUsersInRoleAsync("Agent"))
                .Where(u => u.IsActive)
                .Select(u => new { Id = u.Id, Name = u.FullName })
                .ToList();
        }

        ViewBag.IsAdmin = isAdmin;
        ViewBag.CurrentUserId = currentUser.Id;

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")] // Only Admin can assign agents
    public async Task<IActionResult> Assign(Guid inquiryId, Guid agentId)
    {
        var command = new AssignInquiryCommand
        {
            InquiryId = inquiryId,
            AgentId = agentId
        };

        await _mediator.Send(command);

        TempData["SuccessMessage"] = "Inquiry assigned successfully";
        return RedirectToAction(nameof(Details), new { id = inquiryId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reply(ReplyToInquiryViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Details), new { id = model.InquiryId });
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var command = new AddMessageCommand
        {
            InquiryId = model.InquiryId,
            Message = model.Message,
            SenderType = SenderType.Agent,
            SenderId = currentUser.Id
        };

        await _mediator.Send(command);

        TempData["SuccessMessage"] = "Reply sent successfully";
        return RedirectToAction(nameof(Details), new { id = model.InquiryId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(Guid id)
    {
        var command = new UpdateInquiryStatusCommand
        {
            InquiryId = id,
            Status = InquiryStatus.Closed
        };

        await _mediator.Send(command);

        TempData["SuccessMessage"] = "Inquiry closed successfully";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(Guid inquiryId, InquiryStatus status)
    {
        // Authorization check: Agents can only update status of their own inquiries
        var inquiry = await _mediator.Send(new GetInquiryByIdQuery { Id = inquiryId });
        if (inquiry == null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var isAdmin = User.IsInRole("Admin");
        var isAgent = User.IsInRole("Agent");

        // Agents can only update status of inquiries assigned to them
        if (isAgent && !isAdmin)
        {
            if (inquiry.AssignedToId != currentUser.Id)
            {
                _logger.LogWarning("Agent {AgentId} attempted to update status of inquiry {InquiryId} not assigned to them", currentUser.Id, inquiryId);
                return Forbid();
            }
        }

        var command = new UpdateInquiryStatusCommand
        {
            InquiryId = inquiryId,
            Status = status
        };

        await _mediator.Send(command);

        TempData["SuccessMessage"] = $"Status updated to {status} successfully";
        return RedirectToAction(nameof(Details), new { id = inquiryId });
    }
}
