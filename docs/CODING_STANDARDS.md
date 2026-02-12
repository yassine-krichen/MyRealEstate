# 📐 EstateFlow Coding Standards & Patterns

## 🎨 Code Style Guide

### Naming Conventions

**Classes & Interfaces:**
```csharp
// Classes: PascalCase, descriptive nouns
public class DealStatistics { }
public class PropertyImage { }

// Interfaces: I + PascalCase
public interface IApplicationDbContext { }
public interface ICurrentUserService { }

// Commands: {Verb}{Entity}Command
public class CreateDealCommand { }
public class UpdatePropertyCommand { }
public class DeleteInquiryCommand { }

// Queries: Get{Entity}{Filter/By}Query
public class GetDealByIdQuery { }
public class GetPropertiesQuery { }
public class GetDealStatisticsQuery { }

// Handlers: {Command/Query}Handler
public class CreateDealCommandHandler { }
public class GetDealByIdQueryHandler { }

// DTOs: {Entity}{Purpose}Dto
public class DealDetailDto { }
public class DealListDto { }
public class PropertySummaryDto { }
```

**Properties & Fields:**
```csharp
// Properties: PascalCase
public string BuyerName { get; set; }
public DateTime CreatedAt { get; set; }

// Private fields: _camelCase
private readonly IApplicationDbContext _context;
private readonly ILogger<CreateDealCommandHandler> _logger;

// Local variables: camelCase
var dealId = Guid.NewGuid();
var commissionAmount = CalculateCommission();
```

**Methods:**
```csharp
// Public methods: PascalCase, verb-first
public async Task<Guid> Handle(...)
public async Task<IActionResult> Create(...)
public decimal CalculateCommission(decimal salePrice, decimal rate)

// Private methods: PascalCase
private async Task UpdatePropertyStatus(...)
private void ValidateBusinessRules(...)
```

### File Organization

**One class per file**, filename matches class name:
```
✅ CreateDealCommand.cs          → contains CreateDealCommand class
✅ CreateDealCommandHandler.cs   → contains CreateDealCommandHandler class
✅ DealDetailDto.cs              → contains DealDetailDto class

❌ DealCommands.cs               → contains multiple command classes (bad)
```

**Namespace matches folder structure:**
```csharp
// File: Application/Commands/Deals/CreateDealCommand.cs
namespace MyRealEstate.Application.Commands.Deals;

// File: Application/DTOs/DealDetailDto.cs
namespace MyRealEstate.Application.DTOs;

// File: Web/Areas/Admin/Controllers/DealsController.cs
namespace MyRealEstate.Web.Areas.Admin.Controllers;
```

### Using Statements

**Order:**
1. System namespaces
2. Third-party namespaces (MediatR, Microsoft.Extensions, etc.)
3. Project namespaces
4. Blank line before namespace declaration

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyRealEstate.Application.Interfaces;
using MyRealEstate.Domain.Entities;
using MyRealEstate.Domain.Enums;

namespace MyRealEstate.Application.Commands.Deals;
```

## 🏗️ Architecture Patterns

### CQRS with MediatR

**Command Pattern:**
```csharp
// Command: Represents an intent to change state
public class CreateDealCommand : IRequest<Guid>
{
    [Required]
    public Guid PropertyId { get; set; }
    
    [Required]
    [StringLength(200)]
    public string BuyerName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string BuyerEmail { get; set; } = string.Empty;
    
    [Range(0.01, double.MaxValue, ErrorMessage = "Sale price must be positive")]
    public decimal SalePrice { get; set; }
    
    [Range(0, 100, ErrorMessage = "Commission rate must be between 0 and 100")]
    public decimal CommissionRate { get; set; } = 5.0m;
    
    public string? Notes { get; set; }
}

// Handler: Executes the command
public class CreateDealCommandHandler : IRequestHandler<CreateDealCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateDealCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService;
    
    public CreateDealCommandHandler(
        IApplicationDbContext context,
        ILogger<CreateDealCommandHandler> logger,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _logger = logger;
        _currentUserService = currentUserService;
    }
    
    public async Task<Guid> Handle(CreateDealCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate
        var property = await _context.Properties
            .FirstOrDefaultAsync(p => p.Id == request.PropertyId, cancellationToken)
            ?? throw new KeyNotFoundException($"Property with ID {request.PropertyId} not found");
        
        if (property.Status == PropertyStatus.Sold)
            throw new InvalidOperationException("Cannot create deal for already sold property");
        
        // 2. Create entity
        var deal = new Deal
        {
            Id = Guid.NewGuid(),
            PropertyId = request.PropertyId,
            AgentId = _currentUserService.UserId ?? throw new InvalidOperationException("User not authenticated"),
            BuyerName = request.BuyerName,
            BuyerEmail = request.BuyerEmail,
            SalePrice = request.SalePrice,
            CommissionRate = request.CommissionRate,
            CommissionAmount = request.SalePrice * (request.CommissionRate / 100),
            Status = DealStatus.Draft,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        // 3. Update related entities
        property.Status = PropertyStatus.Sold;
        property.ClosedDealId = deal.Id;
        property.UpdatedAt = DateTime.UtcNow;
        
        // 4. Save
        _context.Deals.Add(deal);
        await _context.SaveChangesAsync(cancellationToken);
        
        // 5. Log
        _logger.LogInformation(
            "Deal {DealId} created for Property {PropertyId} by Agent {AgentId}",
            deal.Id, property.Id, deal.AgentId);
        
        return deal.Id;
    }
}
```

**Query Pattern:**
```csharp
// Query: Represents a request for data
public class GetDealByIdQuery : IRequest<DealDetailDto?>
{
    public Guid Id { get; set; }
}

// Handler: Retrieves and transforms data
public class GetDealByIdQueryHandler : IRequestHandler<GetDealByIdQuery, DealDetailDto?>
{
    private readonly IApplicationDbContext _context;
    
    public GetDealByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<DealDetailDto?> Handle(GetDealByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Deals
            .AsNoTracking()
            .Include(d => d.Property)
                .ThenInclude(p => p.PropertyImages)
            .Include(d => d.Agent)
            .Include(d => d.Inquiry)
            .Where(d => d.Id == request.Id)
            .Select(d => new DealDetailDto
            {
                Id = d.Id,
                PropertyId = d.PropertyId,
                PropertyTitle = d.Property.Title,
                PropertyAddress = d.Property.Address.ToString(),
                PropertyMainImage = d.Property.PropertyImages
                    .Where(i => i.IsMainImage)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault(),
                AgentId = d.AgentId,
                AgentName = d.Agent.FullName,
                AgentEmail = d.Agent.Email ?? string.Empty,
                BuyerName = d.BuyerName,
                BuyerEmail = d.BuyerEmail,
                BuyerPhone = d.BuyerPhone,
                SalePrice = d.SalePrice,
                CommissionRate = d.CommissionRate,
                CommissionAmount = d.CommissionAmount,
                Status = d.Status,
                Notes = d.Notes,
                ClosedAt = d.ClosedAt,
                CreatedAt = d.CreatedAt,
                InquiryId = d.InquiryId
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
```

### Repository Pattern (Use Sparingly)

**Only create repositories for complex queries that don't fit CQRS pattern well.**

```csharp
// Interface in Application layer
public interface IDealRepository
{
    Task<List<DealStatistics>> GetMonthlyRevenueAsync(DateTime fromDate, DateTime toDate);
}

// Implementation in Infrastructure layer
public class DealRepository : IDealRepository
{
    private readonly ApplicationDbContext _context;
    
    public DealRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<DealStatistics>> GetMonthlyRevenueAsync(DateTime fromDate, DateTime toDate)
    {
        // Complex query that doesn't fit well in a Query Handler
        return await _context.Deals
            .Where(d => d.Status == DealStatus.Completed)
            .Where(d => d.ClosedAt >= fromDate && d.ClosedAt <= toDate)
            .GroupBy(d => new { d.ClosedAt!.Value.Year, d.ClosedAt.Value.Month })
            .Select(g => new DealStatistics
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalRevenue = g.Sum(d => d.SalePrice),
                DealCount = g.Count()
            })
            .OrderBy(s => s.Year)
            .ThenBy(s => s.Month)
            .ToListAsync();
    }
}
```

## 🎯 Entity Framework Patterns

### DbContext Configuration

**Entity configurations in separate files:**
```csharp
// Infrastructure/Data/Configurations/DealConfiguration.cs
public class DealConfiguration : IEntityTypeConfiguration<Deal>
{
    public void Configure(EntityTypeBuilder<Deal> builder)
    {
        builder.HasKey(d => d.Id);
        
        builder.Property(d => d.BuyerName)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(d => d.BuyerEmail)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(d => d.SalePrice)
            .HasPrecision(18, 2);
        
        builder.Property(d => d.CommissionRate)
            .HasPrecision(5, 2);
        
        builder.Property(d => d.CommissionAmount)
            .HasPrecision(18, 2);
        
        builder.Property(d => d.Status)
            .HasConversion<int>();
        
        // Relationships
        builder.HasOne(d => d.Property)
            .WithOne(p => p.ClosedDeal)
            .HasForeignKey<Deal>(d => d.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(d => d.Agent)
            .WithMany(u => u.Deals)
            .HasForeignKey(d => d.AgentId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(d => d.Inquiry)
            .WithOne(i => i.RelatedDeal)
            .HasForeignKey<Deal>(d => d.InquiryId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Indexes
        builder.HasIndex(d => d.PropertyId);
        builder.HasIndex(d => d.AgentId);
        builder.HasIndex(d => d.Status);
        builder.HasIndex(d => d.ClosedAt);
    }
}
```

### Query Patterns

**Use AsNoTracking for read-only queries:**
```csharp
// ✅ GOOD: Read-only, no tracking overhead
var deals = await _context.Deals
    .AsNoTracking()
    .Where(d => d.Status == DealStatus.Completed)
    .ToListAsync();

// ❌ BAD: Tracking entities that won't be modified
var deals = await _context.Deals
    .Where(d => d.Status == DealStatus.Completed)
    .ToListAsync();
```

**Use projection for list views:**
```csharp
// ✅ GOOD: Only select needed fields
var deals = await _context.Deals
    .AsNoTracking()
    .Select(d => new DealListDto
    {
        Id = d.Id,
        PropertyTitle = d.Property.Title,
        BuyerName = d.BuyerName,
        SalePrice = d.SalePrice,
        Status = d.Status,
        ClosedAt = d.ClosedAt
    })
    .ToListAsync();

// ❌ BAD: Loading entire entity when only need few fields
var deals = await _context.Deals
    .AsNoTracking()
    .Include(d => d.Property)
    .Include(d => d.Agent)
    .Include(d => d.Inquiry)
    .ToListAsync();
```

**Use Include wisely:**
```csharp
// ✅ GOOD: Only include what you need
var deal = await _context.Deals
    .Include(d => d.Property)
        .ThenInclude(p => p.PropertyImages.Where(i => i.IsMainImage))
    .Include(d => d.Agent)
    .FirstOrDefaultAsync(d => d.Id == id);

// ❌ BAD: Including everything
var deal = await _context.Deals
    .Include(d => d.Property)
        .ThenInclude(p => p.PropertyImages)
        .ThenInclude(i => i.Property) // Circular reference!
    .Include(d => d.Agent)
        .ThenInclude(a => a.Properties) // Unnecessary
    .Include(d => d.Inquiry)
        .ThenInclude(i => i.Messages) // Not needed
    .FirstOrDefaultAsync(d => d.Id == id);
```

## 🛡️ Validation & Error Handling

### Data Annotations
```csharp
public class CreateDealCommand : IRequest<Guid>
{
    [Required(ErrorMessage = "Property is required")]
    public Guid PropertyId { get; set; }
    
    [Required(ErrorMessage = "Buyer name is required")]
    [StringLength(200, ErrorMessage = "Buyer name cannot exceed 200 characters")]
    public string BuyerName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string BuyerEmail { get; set; } = string.Empty;
    
    [Phone(ErrorMessage = "Invalid phone format")]
    public string? BuyerPhone { get; set; }
    
    [Range(0.01, double.MaxValue, ErrorMessage = "Sale price must be greater than 0")]
    [DataType(DataType.Currency)]
    public decimal SalePrice { get; set; }
    
    [Range(0, 100, ErrorMessage = "Commission rate must be between 0 and 100")]
    public decimal CommissionRate { get; set; } = 5.0m;
}
```

### Business Rule Validation
```csharp
// In command handler
public async Task<Guid> Handle(CreateDealCommand request, CancellationToken cancellationToken)
{
    // Validate entity exists
    var property = await _context.Properties
        .FirstOrDefaultAsync(p => p.Id == request.PropertyId, cancellationToken)
        ?? throw new KeyNotFoundException($"Property with ID {request.PropertyId} not found");
    
    // Validate business rules
    if (property.Status == PropertyStatus.Sold)
        throw new InvalidOperationException($"Property '{property.Title}' is already sold");
    
    if (property.Status == PropertyStatus.Draft)
        throw new InvalidOperationException("Cannot create deal for draft property");
    
    // Validate agent assignment
    var agent = await _context.Users
        .FirstOrDefaultAsync(u => u.Id == request.AgentId, cancellationToken)
        ?? throw new KeyNotFoundException($"Agent with ID {request.AgentId} not found");
    
    if (!await _context.UserRoles.AnyAsync(
        ur => ur.UserId == agent.Id && 
        _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Agent"),
        cancellationToken))
    {
        throw new InvalidOperationException("User is not an agent");
    }
    
    // Continue with creation...
}
```

### Controller Error Handling
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(CreateDealCommand command)
{
    // Check model state
    if (!ModelState.IsValid)
    {
        // Reload dropdowns/data needed for form
        await PopulateViewBag();
        return View(command);
    }
    
    try
    {
        var dealId = await _mediator.Send(command);
        
        TempData["Success"] = "Deal created successfully";
        _logger.LogInformation("Deal {DealId} created successfully", dealId);
        
        return RedirectToAction(nameof(Details), new { id = dealId });
    }
    catch (KeyNotFoundException ex)
    {
        _logger.LogWarning(ex, "Entity not found when creating deal");
        ModelState.AddModelError("", ex.Message);
        await PopulateViewBag();
        return View(command);
    }
    catch (InvalidOperationException ex)
    {
        _logger.LogWarning(ex, "Business rule violation when creating deal");
        ModelState.AddModelError("", ex.Message);
        await PopulateViewBag();
        return View(command);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error creating deal");
        TempData["Error"] = "An unexpected error occurred. Please try again.";
        await PopulateViewBag();
        return View(command);
    }
}
```

## 📝 Logging Best Practices

```csharp
// ✅ GOOD: Structured logging with context
_logger.LogInformation(
    "Deal {DealId} created for Property {PropertyId} by Agent {AgentId} with sale price {SalePrice}",
    deal.Id, deal.PropertyId, deal.AgentId, deal.SalePrice);

_logger.LogWarning(
    "Attempt to create deal for already sold property {PropertyId}",
    request.PropertyId);

_logger.LogError(ex,
    "Error updating property status for deal {DealId}",
    dealId);

// ❌ BAD: String concatenation, no context
_logger.LogInformation("Deal created: " + dealId);
_logger.LogError("Error: " + ex.Message);
```

## 🎨 UI Patterns

### Bootstrap Components
```cshtml
@* Status badges *@
@switch (Model.Status)
{
    case DealStatus.Draft:
        <span class="badge bg-secondary">Draft</span>
        break;
    case DealStatus.Completed:
        <span class="badge bg-success">Completed</span>
        break;
    case DealStatus.Cancelled:
        <span class="badge bg-danger">Cancelled</span>
        break;
}

@* Card with EstateFlow header *@
<div class="card shadow-sm mb-4">
    <div class="card-header" style="background-color: var(--estate-primary); color: white;">
        <h5 class="mb-0"><i class="bi bi-cash-coin"></i> Deal Information</h5>
    </div>
    <div class="card-body">
        @* Content *@
    </div>
</div>

@* Confirmation modal *@
<div class="modal fade" id="completeModal" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">Complete Deal</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                Are you sure you want to mark this deal as completed?
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                <form asp-action="Complete" method="post">
                    <input type="hidden" name="id" value="@Model.Id" />
                    <button type="submit" class="btn btn-success">
                        <i class="bi bi-check-circle"></i> Complete
                    </button>
                </form>
            </div>
        </div>
    </div>
</div>
```

---

**Remember: Consistency is more important than personal preference. Follow the existing patterns!**
