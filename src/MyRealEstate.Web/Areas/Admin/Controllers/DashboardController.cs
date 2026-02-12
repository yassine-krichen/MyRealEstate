using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyRealEstate.Application.Interfaces;
using MyRealEstate.Application.Queries.Analytics;
using MyRealEstate.Domain.Enums;

namespace MyRealEstate.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Agent")]
public class DashboardController : Controller
{
    private readonly IApplicationDbContext _context;
    private readonly IMediator _mediator;

    public DashboardController(IApplicationDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<IActionResult> Index()
    {
        var isAdmin = User.IsInRole("Admin");
        var isAgent = User.IsInRole("Agent");
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        Guid? currentUserId = userId != null ? Guid.Parse(userId) : null;

        // Base queries
        var propertiesQuery = _context.Properties.AsQueryable();
        var inquiriesQuery = _context.Inquiries.AsQueryable();
        var dealsQuery = _context.Deals.AsQueryable();

        // Filter for agents: only their assigned inquiries + unassigned new ones
        if (isAgent && !isAdmin && currentUserId.HasValue)
        {
            inquiriesQuery = inquiriesQuery.Where(i => 
                i.AssignedAgentId == currentUserId.Value || 
                (i.AssignedAgentId == null && i.Status == InquiryStatus.New));
        }

        var viewModel = new DashboardViewModel
        {
            TotalProperties = await propertiesQuery.CountAsync(),
            PublishedProperties = await propertiesQuery.CountAsync(p => p.Status == PropertyStatus.Published),
            DraftProperties = await propertiesQuery.CountAsync(p => p.Status == PropertyStatus.Draft),
            SoldProperties = await propertiesQuery.CountAsync(p => p.Status == PropertyStatus.Sold),
            
            TotalInquiries = await inquiriesQuery.CountAsync(),
            NewInquiries = await inquiriesQuery.CountAsync(i => i.Status == InquiryStatus.New),
            OpenInquiries = await inquiriesQuery.CountAsync(i => 
                i.Status != InquiryStatus.Closed && i.Status != InquiryStatus.Answered),
            
            TotalDeals = await dealsQuery.CountAsync(),
            PendingDeals = await dealsQuery.CountAsync(d => d.Status == DealStatus.Pending),
            CompletedDeals = await dealsQuery.CountAsync(d => d.Status == DealStatus.Completed),
            DealsThisMonth = await dealsQuery.CountAsync(d => 
                d.ClosedAt >= DateTime.UtcNow.AddDays(-30) && d.Status == DealStatus.Completed),
            
            RecentDeals = await dealsQuery
                .Include(d => d.Property)
                .Include(d => d.Agent)
                .OrderByDescending(d => d.CreatedAt)
                .Take(5)
                .ToListAsync(),
            
            RecentInquiries = await inquiriesQuery
                .Include(i => i.Property)
                .OrderByDescending(i => i.CreatedAt)
                .Take(5)
                .ToListAsync(),
            
            RecentProperties = await propertiesQuery
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToListAsync()
        };

        // Compute deal financials client-side (SQLite doesn't support SUM on decimal)
        var completedDealFinancials = await dealsQuery
            .Where(d => d.Status == DealStatus.Completed)
            .Select(d => new { d.SalePrice, d.CommissionAmount })
            .ToListAsync();
        viewModel.TotalRevenue = completedDealFinancials.Sum(d => d.SalePrice);
        viewModel.TotalCommission = completedDealFinancials.Sum(d => d.CommissionAmount ?? 0);

        // Get most viewed properties
        var mostViewedQuery = new GetMostViewedPropertiesQuery
        {
            TopCount = 5,
            FromDate = DateTime.UtcNow.AddDays(-30) // Last 30 days
        };
        viewModel.MostViewedProperties = await _mediator.Send(mostViewedQuery);

        return View(viewModel);
    }
}

public class DashboardViewModel
{
    public int TotalProperties { get; set; }
    public int PublishedProperties { get; set; }
    public int DraftProperties { get; set; }
    public int SoldProperties { get; set; }
    
    public int TotalInquiries { get; set; }
    public int NewInquiries { get; set; }
    public int OpenInquiries { get; set; }
    
    public int TotalDeals { get; set; }
    public int PendingDeals { get; set; }
    public int CompletedDeals { get; set; }
    public int DealsThisMonth { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCommission { get; set; }
    
    public List<MyRealEstate.Domain.Entities.Deal> RecentDeals { get; set; } = new();
    public List<MyRealEstate.Domain.Entities.Inquiry> RecentInquiries { get; set; } = new();
    public List<MyRealEstate.Domain.Entities.Property> RecentProperties { get; set; } = new();
    public List<MyRealEstate.Application.Interfaces.PropertyViewStats> MostViewedProperties { get; set; } = new();
}
