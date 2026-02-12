using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyRealEstate.Application.Commands.Deals;
using MyRealEstate.Application.Interfaces;
using MyRealEstate.Application.Queries.Deals;
using MyRealEstate.Application.Queries.Inquiries;
using MyRealEstate.Domain.Entities;
using MyRealEstate.Domain.Enums;
using MyRealEstate.Web.Models;

namespace MyRealEstate.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Agent")]
public class DealsController : Controller
{
    private readonly IMediator _mediator;
    private readonly UserManager<User> _userManager;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DealsController> _logger;

    public DealsController(
        IMediator mediator,
        UserManager<User> userManager,
        IApplicationDbContext context,
        ILogger<DealsController> logger)
    {
        _mediator = mediator;
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    // GET: Admin/Deals
    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, DealStatus? status = null, Guid? agentId = null, string? searchTerm = null)
    {
        var query = new GetAllDealsQuery
        {
            Page = page,
            PageSize = 20,
            Status = status,
            AgentId = agentId,
            SearchTerm = searchTerm
        };

        var result = await _mediator.Send(query);

        var viewModel = new DealSearchViewModel
        {
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            Status = status,
            AgentId = agentId,
            SearchTerm = searchTerm,
            Results = result.Items.Select(d => new DealListViewModel
            {
                Id = d.Id,
                PropertyTitle = d.PropertyTitle,
                PropertyCity = d.PropertyCity,
                BuyerName = d.BuyerName,
                BuyerEmail = d.BuyerEmail,
                AgentName = d.AgentName,
                SalePrice = d.SalePrice,
                CommissionAmount = d.CommissionAmount,
                Status = d.Status,
                CreatedAt = d.CreatedAt,
                ClosedAt = d.ClosedAt
            }).ToList()
        };

        // Get agents for filter dropdown
        ViewBag.Agents = (await _userManager.GetUsersInRoleAsync("Agent"))
            .Where(u => u.IsActive)
            .Select(u => new { Id = u.Id, Name = u.FullName })
            .ToList();

        return View(viewModel);
    }

    // GET: Admin/Deals/Details/5
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var query = new GetDealByIdQuery { Id = id };
        var deal = await _mediator.Send(query);

        if (deal == null)
        {
            return NotFound();
        }

        var viewModel = new DealDetailViewModel
        {
            Id = deal.Id,
            PropertyId = deal.PropertyId,
            PropertyTitle = deal.PropertyTitle,
            PropertyCity = deal.PropertyCity,
            PropertyMainImageUrl = deal.PropertyMainImageUrl,
            AgentId = deal.AgentId,
            AgentName = deal.AgentName,
            AgentEmail = deal.AgentEmail,
            AgentPhone = deal.AgentPhone,
            InquiryId = deal.InquiryId,
            InquiryVisitorName = deal.InquiryVisitorName,
            BuyerName = deal.BuyerName,
            BuyerEmail = deal.BuyerEmail,
            BuyerPhone = deal.BuyerPhone,
            SalePrice = deal.SalePrice,
            CommissionPercent = deal.CommissionPercent,
            CommissionAmount = deal.CommissionAmount,
            Status = deal.Status,
            Notes = deal.Notes,
            CreatedAt = deal.CreatedAt,
            UpdatedAt = deal.UpdatedAt,
            ClosedAt = deal.ClosedAt
        };

        return View(viewModel);
    }

    // GET: Admin/Deals/Create
    [HttpGet]
    public async Task<IActionResult> Create(Guid? inquiryId = null)
    {
        var viewModel = new DealCreateViewModel();

        // If creating from an inquiry, pre-fill the form
        if (inquiryId.HasValue)
        {
            var inquiry = await _mediator.Send(new GetInquiryByIdQuery { Id = inquiryId.Value });
            if (inquiry != null)
            {
                viewModel.InquiryId = inquiryId;
                viewModel.PropertyId = inquiry.PropertyId;
                viewModel.BuyerName = inquiry.ClientName;
                viewModel.BuyerEmail = inquiry.ClientEmail;
                viewModel.BuyerPhone = inquiry.ClientPhone;
            }
        }

        await PopulateCreateViewBag();
        return View(viewModel);
    }

    // POST: Admin/Deals/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DealCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCreateViewBag();
            return View(model);
        }

        try
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var command = new CreateDealCommand
            {
                PropertyId = model.PropertyId,
                InquiryId = model.InquiryId,
                AgentId = currentUser.Id,
                BuyerName = model.BuyerName,
                BuyerEmail = model.BuyerEmail,
                BuyerPhone = model.BuyerPhone,
                SalePrice = model.SalePrice,
                CommissionRate = model.CommissionRate,
                Notes = model.Notes
            };

            var dealId = await _mediator.Send(command);

            TempData["Success"] = "Deal created successfully";
            return RedirectToAction(nameof(Details), new { id = dealId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateCreateViewBag();
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating deal");
            TempData["Error"] = "Failed to create deal. Please try again.";
            await PopulateCreateViewBag();
            return View(model);
        }
    }

    // GET: Admin/Deals/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var deal = await _mediator.Send(new GetDealByIdQuery { Id = id });

        if (deal == null)
        {
            return NotFound();
        }

        if (deal.Status != DealStatus.Pending)
        {
            TempData["Error"] = "Only pending deals can be edited";
            return RedirectToAction(nameof(Details), new { id });
        }

        var viewModel = new DealEditViewModel
        {
            Id = deal.Id,
            Status = deal.Status,
            PropertyTitle = deal.PropertyTitle,
            BuyerName = deal.BuyerName ?? string.Empty,
            BuyerEmail = deal.BuyerEmail ?? string.Empty,
            BuyerPhone = deal.BuyerPhone,
            SalePrice = deal.SalePrice,
            CommissionRate = deal.CommissionPercent ?? 5.0m,
            Notes = deal.Notes
        };

        return View(viewModel);
    }

    // POST: Admin/Deals/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, DealEditViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var command = new UpdateDealCommand
            {
                Id = model.Id,
                BuyerName = model.BuyerName,
                BuyerEmail = model.BuyerEmail,
                BuyerPhone = model.BuyerPhone,
                SalePrice = model.SalePrice,
                CommissionRate = model.CommissionRate,
                Notes = model.Notes
            };

            await _mediator.Send(command);

            TempData["Success"] = "Deal updated successfully";
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating deal {DealId}", id);
            TempData["Error"] = "Failed to update deal. Please try again.";
            return View(model);
        }
    }

    // POST: Admin/Deals/Complete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(Guid id, string? notes = null)
    {
        try
        {
            var command = new CompleteDealCommand
            {
                Id = id,
                Notes = notes
            };

            await _mediator.Send(command);

            TempData["Success"] = "Deal completed successfully";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing deal {DealId}", id);
            TempData["Error"] = "Failed to complete deal. Please try again.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: Admin/Deals/Cancel/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id, string? cancellationReason = null)
    {
        try
        {
            var command = new CancelDealCommand
            {
                Id = id,
                CancellationReason = cancellationReason
            };

            await _mediator.Send(command);

            TempData["Success"] = "Deal cancelled successfully";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling deal {DealId}", id);
            TempData["Error"] = "Failed to cancel deal. Please try again.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // AJAX: Admin/Deals/GetInquiriesForProperty
    [HttpGet]
    public async Task<IActionResult> GetInquiriesForProperty(Guid propertyId)
    {
        var result = await _mediator.Send(new GetInquiriesQuery
        {
            PropertyId = propertyId,
            PageSize = 100
        });

        var inquiries = result.Items
            .Where(i => i.Status != InquiryStatus.Closed)
            .Select(i => new { id = i.Id, name = $"{i.ClientName} ({i.ClientEmail})" })
            .ToList();

        return Json(inquiries);
    }

    private async Task PopulateCreateViewBag()
    {
        // Get available properties (not sold, not deleted)
        ViewBag.Properties = await _context.Properties
            .Where(p => !p.IsDeleted && p.Status != PropertyStatus.Sold)
            .OrderBy(p => p.Title)
            .Select(p => new { Id = p.Id, Title = $"{p.Title} - {p.Address.City}" })
            .ToListAsync();

        // Get agents
        ViewBag.Agents = (await _userManager.GetUsersInRoleAsync("Agent"))
            .Where(u => u.IsActive)
            .Select(u => new { Id = u.Id, Name = u.FullName })
            .ToList();
    }
}
