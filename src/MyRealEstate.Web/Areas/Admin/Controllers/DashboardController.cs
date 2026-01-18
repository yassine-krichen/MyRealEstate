using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyRealEstate.Domain.Enums;
using MyRealEstate.Infrastructure.Data;

namespace MyRealEstate.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Agent")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = new DashboardViewModel
        {
            TotalProperties = await _context.Properties.CountAsync(),
            PublishedProperties = await _context.Properties.CountAsync(p => p.Status == PropertyStatus.Published),
            DraftProperties = await _context.Properties.CountAsync(p => p.Status == PropertyStatus.Draft),
            SoldProperties = await _context.Properties.CountAsync(p => p.Status == PropertyStatus.Sold),
            
            TotalInquiries = await _context.Inquiries.CountAsync(),
            NewInquiries = await _context.Inquiries.CountAsync(i => i.Status == InquiryStatus.New),
            OpenInquiries = await _context.Inquiries.CountAsync(i => 
                i.Status != InquiryStatus.Closed && i.Status != InquiryStatus.Answered),
            
            TotalDeals = await _context.Deals.CountAsync(),
            DealsThisMonth = await _context.Deals.CountAsync(d => 
                d.ClosedAt >= DateTime.UtcNow.AddDays(-30) && d.Status == DealStatus.Completed),
            
            RecentInquiries = await _context.Inquiries
                .Include(i => i.Property)
                .OrderByDescending(i => i.CreatedAt)
                .Take(5)
                .ToListAsync(),
            
            RecentProperties = await _context.Properties
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToListAsync()
        };

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
    public int DealsThisMonth { get; set; }
    
    public List<MyRealEstate.Domain.Entities.Inquiry> RecentInquiries { get; set; } = new();
    public List<MyRealEstate.Domain.Entities.Property> RecentProperties { get; set; } = new();
}
