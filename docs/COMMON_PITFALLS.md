# ⚠️ Common Pitfalls & Troubleshooting Guide

## 🚨 Critical Mistakes to Avoid

### 1. Breaking Clean Architecture

**❌ WRONG: Domain layer depending on outer layers**
```csharp
// In MyRealEstate.Domain/Entities/Deal.cs
using Microsoft.EntityFrameworkCore; // ❌ EF Core is Infrastructure concern!
using MyRealEstate.Application.DTOs; // ❌ Domain can't depend on Application!

public class Deal
{
    [Column(TypeName = "decimal(18,2)")] // ❌ Data annotation from EF Core!
    public decimal SalePrice { get; set; }
}
```

**✅ CORRECT: Domain layer is pure**
```csharp
// In MyRealEstate.Domain/Entities/Deal.cs
// NO using statements except System.*

public class Deal
{
    public decimal SalePrice { get; set; } // ✅ Pure property
}

// Configuration goes in Infrastructure layer
// Infrastructure/Data/Configurations/DealConfiguration.cs
public class DealConfiguration : IEntityTypeConfiguration<Deal>
{
    public void Configure(EntityTypeBuilder<Deal> builder)
    {
        builder.Property(d => d.SalePrice).HasPrecision(18, 2); // ✅ Correct place
    }
}
```

---

### 2. Forgetting Soft Delete

**❌ WRONG: Hard deleting entities**
```csharp
public async Task<Unit> Handle(DeleteDealCommand request, CancellationToken cancellationToken)
{
    var deal = await _context.Deals.FindAsync(request.Id);
    _context.Deals.Remove(deal); // ❌ Hard delete!
    await _context.SaveChangesAsync(cancellationToken);
    return Unit.Value;
}
```

**✅ CORRECT: Soft delete with ISoftDelete interface**
```csharp
public async Task<Unit> Handle(DeleteDealCommand request, CancellationToken cancellationToken)
{
    var deal = await _context.Deals.FindAsync(request.Id)
        ?? throw new KeyNotFoundException($"Deal with ID {request.Id} not found");
    
    deal.IsDeleted = true; // ✅ Soft delete
    deal.DeletedAt = DateTime.UtcNow;
    deal.UpdatedAt = DateTime.UtcNow;
    
    await _context.SaveChangesAsync(cancellationToken);
    return Unit.Value;
}
```

---

### 3. Not Using Transactions for Multi-Entity Updates

**❌ WRONG: No transaction - data can be inconsistent**
```csharp
public async Task<Guid> Handle(CreateDealCommand request, CancellationToken cancellationToken)
{
    var deal = new Deal { /* ... */ };
    _context.Deals.Add(deal);
    await _context.SaveChangesAsync(cancellationToken); // ✅ Deal saved
    
    var property = await _context.Properties.FindAsync(request.PropertyId);
    property.Status = PropertyStatus.Sold;
    property.ClosedDealId = deal.Id;
    await _context.SaveChangesAsync(cancellationToken); // ❌ If this fails, deal exists but property not updated!
    
    return deal.Id;
}
```

**✅ CORRECT: Use transaction for atomicity**
```csharp
public async Task<Guid> Handle(CreateDealCommand request, CancellationToken cancellationToken)
{
    await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    
    try
    {
        var deal = new Deal { /* ... */ };
        _context.Deals.Add(deal);
        
        var property = await _context.Properties.FindAsync(request.PropertyId);
        property!.Status = PropertyStatus.Sold;
        property.ClosedDealId = deal.Id;
        
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken); // ✅ All or nothing
        
        return deal.Id;
    }
    catch
    {
        await transaction.RollbackAsync(cancellationToken);
        throw;
    }
}
```

---

### 4. Not Validating Business Rules

**❌ WRONG: Assuming data is valid**
```csharp
public async Task<Guid> Handle(CreateDealCommand request, CancellationToken cancellationToken)
{
    var property = await _context.Properties.FindAsync(request.PropertyId);
    
    var deal = new Deal
    {
        PropertyId = request.PropertyId,
        // ...
    };
    
    _context.Deals.Add(deal); // ❌ What if property is null? What if already sold?
    await _context.SaveChangesAsync(cancellationToken);
    return deal.Id;
}
```

**✅ CORRECT: Validate everything**
```csharp
public async Task<Guid> Handle(CreateDealCommand request, CancellationToken cancellationToken)
{
    // Validate property exists
    var property = await _context.Properties
        .FirstOrDefaultAsync(p => p.Id == request.PropertyId, cancellationToken)
        ?? throw new KeyNotFoundException($"Property with ID {request.PropertyId} not found");
    
    // Validate property is not already sold
    if (property.Status == PropertyStatus.Sold)
        throw new InvalidOperationException($"Property '{property.Title}' is already sold");
    
    // Validate property is available for sale
    if (property.Status == PropertyStatus.Draft)
        throw new InvalidOperationException("Cannot create deal for draft property");
    
    // Validate agent exists and has Agent role
    var agent = await _context.Users
        .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
        .FirstOrDefaultAsync(u => u.Id == request.AgentId, cancellationToken)
        ?? throw new KeyNotFoundException($"Agent with ID {request.AgentId} not found");
    
    if (!agent.UserRoles.Any(ur => ur.Role.Name == "Agent"))
        throw new InvalidOperationException("User is not an agent");
    
    // Now safe to create deal
    var deal = new Deal { /* ... */ };
    // ...
}
```

---

### 5. Returning Entities from Controllers

**❌ WRONG: Exposing domain entities directly**
```csharp
[HttpGet("{id}")]
public async Task<IActionResult> Details(Guid id)
{
    var deal = await _context.Deals
        .Include(d => d.Property)
        .Include(d => d.Agent)
        .FirstOrDefaultAsync(d => d.Id == id);
    
    return View(deal); // ❌ Exposing entity with navigation properties, EF tracking, etc.
}
```

**✅ CORRECT: Use DTOs**
```csharp
[HttpGet("{id}")]
public async Task<IActionResult> Details(Guid id)
{
    var query = new GetDealByIdQuery { Id = id };
    var deal = await _mediator.Send(query); // ✅ Returns DealDetailDto
    
    if (deal == null)
    {
        TempData["Error"] = "Deal not found";
        return RedirectToAction(nameof(Index));
    }
    
    return View(deal); // ✅ Clean DTO, no tracking, only needed data
}
```

---

### 6. Not Using Cancellation Tokens

**❌ WRONG: Ignoring cancellation tokens**
```csharp
public async Task<Guid> Handle(CreateDealCommand request, CancellationToken cancellationToken)
{
    var deal = await _context.Deals.ToListAsync(); // ❌ No cancellation token!
    // ...
}
```

**✅ CORRECT: Always pass cancellation tokens**
```csharp
public async Task<Guid> Handle(CreateDealCommand request, CancellationToken cancellationToken)
{
    var deal = await _context.Deals.ToListAsync(cancellationToken); // ✅ Proper
    // ...
}
```

---

### 7. Missing Authorization

**❌ WRONG: No authorization on admin actions**
```csharp
public class DealsController : Controller // ❌ Anyone can access!
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateDealCommand command)
    {
        // ...
    }
}
```

**✅ CORRECT: Proper authorization**
```csharp
[Area("Admin")]
[Authorize(Roles = "Admin,Agent")] // ✅ Role-based protection
public class DealsController : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken] // ✅ CSRF protection
    public async Task<IActionResult> Create(CreateDealCommand command)
    {
        // ...
    }
}
```

---

### 8. Not Handling Nullable Navigation Properties

**❌ WRONG: Assuming navigation properties are loaded**
```csharp
public async Task<DealDetailDto> Handle(GetDealByIdQuery request, CancellationToken cancellationToken)
{
    var deal = await _context.Deals.FindAsync(request.Id);
    
    return new DealDetailDto
    {
        PropertyTitle = deal.Property.Title, // ❌ NullReferenceException! Property not loaded
        AgentName = deal.Agent.FullName // ❌ NullReferenceException! Agent not loaded
    };
}
```

**✅ CORRECT: Use Include or handle nulls**
```csharp
public async Task<DealDetailDto> Handle(GetDealByIdQuery request, CancellationToken cancellationToken)
{
    var deal = await _context.Deals
        .Include(d => d.Property) // ✅ Explicitly load
        .Include(d => d.Agent)
        .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
    
    if (deal == null)
        return null;
    
    return new DealDetailDto
    {
        PropertyTitle = deal.Property.Title, // ✅ Safe - Property is loaded
        AgentName = deal.Agent.FullName // ✅ Safe - Agent is loaded
    };
}
```

---

### 9. Not Logging Important Operations

**❌ WRONG: Silent operations**
```csharp
public async Task<Guid> Handle(CreateDealCommand request, CancellationToken cancellationToken)
{
    var deal = new Deal { /* ... */ };
    _context.Deals.Add(deal);
    await _context.SaveChangesAsync(cancellationToken);
    return deal.Id; // ❌ No logging!
}
```

**✅ CORRECT: Log important events**
```csharp
public async Task<Guid> Handle(CreateDealCommand request, CancellationToken cancellationToken)
{
    _logger.LogInformation(
        "Creating deal for Property {PropertyId} by Agent {AgentId}",
        request.PropertyId, request.AgentId);
    
    var deal = new Deal { /* ... */ };
    _context.Deals.Add(deal);
    await _context.SaveChangesAsync(cancellationToken);
    
    _logger.LogInformation(
        "Deal {DealId} created successfully with sale price {SalePrice}",
        deal.Id, deal.SalePrice);
    
    return deal.Id; // ✅ Logged
}
```

---

### 10. Not Updating Audit Fields

**❌ WRONG: Forgetting timestamps**
```csharp
var deal = new Deal
{
    Id = Guid.NewGuid(),
    PropertyId = request.PropertyId,
    // ...
    // ❌ No CreatedAt, UpdatedAt!
};
```

**✅ CORRECT: Always set audit fields**
```csharp
var deal = new Deal
{
    Id = Guid.NewGuid(),
    PropertyId = request.PropertyId,
    // ...
    CreatedAt = DateTime.UtcNow, // ✅ Set creation time
    UpdatedAt = DateTime.UtcNow  // ✅ Set update time
};

// When updating
deal.UpdatedAt = DateTime.UtcNow; // ✅ Update timestamp
```

---

## 🔧 Troubleshooting Common Issues

### Issue: "A referential integrity constraint violation occurred"

**Cause:** Trying to delete/update entity that has foreign key references

**Solution:**
```csharp
// Check for references before deleting
var hasReferences = await _context.Deals.AnyAsync(d => d.PropertyId == propertyId);
if (hasReferences)
    throw new InvalidOperationException("Cannot delete property with associated deals");

// Or use DeleteBehavior.Restrict in configuration
builder.HasOne(d => d.Property)
    .WithOne(p => p.ClosedDeal)
    .OnDelete(DeleteBehavior.Restrict); // ✅ Prevents cascade delete
```

---

### Issue: "Sequence contains no elements" when using First()

**Cause:** Using First() when no items exist

**Solution:**
```csharp
// ❌ WRONG: Throws exception if not found
var deal = await _context.Deals.FirstAsync(d => d.Id == id);

// ✅ CORRECT: Returns null if not found
var deal = await _context.Deals.FirstOrDefaultAsync(d => d.Id == id)
    ?? throw new KeyNotFoundException($"Deal with ID {id} not found");
```

---

### Issue: "Cannot access disposed context"

**Cause:** Trying to access navigation properties after context disposed

**Solution:**
```csharp
// ❌ WRONG: Navigation property accessed after query
var deal = await _context.Deals.FindAsync(id);
// ... context disposed ...
var propertyTitle = deal.Property.Title; // ❌ Property not loaded!

// ✅ CORRECT: Load everything you need in the query
var deal = await _context.Deals
    .Include(d => d.Property)
    .FirstOrDefaultAsync(d => d.Id == id);
var propertyTitle = deal.Property.Title; // ✅ Works
```

---

### Issue: "Tracking errors" when updating entities

**Cause:** Multiple instances of same entity being tracked

**Solution:**
```csharp
// ❌ WRONG: Loading same entity twice
var deal1 = await _context.Deals.FindAsync(id);
var deal2 = await _context.Deals.FirstAsync(d => d.Id == id); // ❌ Tracking conflict!

// ✅ CORRECT: Load once or use AsNoTracking for read-only
var dealReadOnly = await _context.Deals.AsNoTracking().FirstAsync(d => d.Id == id);
var dealForUpdate = await _context.Deals.FindAsync(id);
```

---

### Issue: ViewBag data is null in view after validation failure

**Cause:** Not repopulating ViewBag data when returning view with errors

**Solution:**
```csharp
[HttpPost]
public async Task<IActionResult> Create(CreateDealCommand command)
{
    if (!ModelState.IsValid)
    {
        await PopulateViewBag(); // ✅ Repopulate dropdowns!
        return View(command);
    }
    
    try
    {
        // ...
    }
    catch (Exception ex)
    {
        ModelState.AddModelError("", ex.Message);
        await PopulateViewBag(); // ✅ Repopulate here too!
        return View(command);
    }
}

private async Task PopulateViewBag()
{
    ViewBag.Properties = await _context.Properties
        .Where(p => p.Status == PropertyStatus.Available)
        .Select(p => new SelectListItem
        {
            Value = p.Id.ToString(),
            Text = p.Title
        })
        .ToListAsync();
}
```

---

### Issue: TempData is empty on next request

**Cause:** TempData is only available for one redirect

**Solution:**
```csharp
// ✅ Set TempData before redirect
TempData["Success"] = "Deal created successfully";
return RedirectToAction(nameof(Details), new { id = dealId });

// In the view
@if (TempData["Success"] != null)
{
    <div class="alert alert-success">@TempData["Success"]</div>
}

// ❌ Don't try to access TempData multiple times
var msg = TempData["Success"]; // First access - OK
var msg2 = TempData["Success"]; // Second access - NULL!

// ✅ Use Peek to preserve for multiple reads
var msg = TempData.Peek("Success");
```

---

### Issue: JavaScript not working after form submission

**Cause:** Scripts not included in @section Scripts

**Solution:**
```cshtml
@* At bottom of view *@
@section Scripts {
    <partial name="_ValidationScriptsPartial" />
    
    <script>
        // Your custom JavaScript here
        $(document).ready(function() {
            $('#PropertyId').on('change', function() {
                // Load inquiry data
            });
        });
    </script>
}
```

---

### Issue: Decimal values losing precision

**Cause:** Not configuring decimal precision in EF Core

**Solution:**
```csharp
// In DealConfiguration.cs
builder.Property(d => d.SalePrice)
    .HasPrecision(18, 2); // ✅ 18 total digits, 2 after decimal

builder.Property(d => d.CommissionRate)
    .HasPrecision(5, 2); // ✅ 5 total digits (allows 100.00)
```

---

### Issue: DateTime comparison issues in queries

**Cause:** DateTime kind differences (UTC vs Local)

**Solution:**
```csharp
// ✅ ALWAYS use UTC
var now = DateTime.UtcNow; // ✅ Not DateTime.Now

// When comparing dates
var startOfDay = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

var deals = await _context.Deals
    .Where(d => d.CreatedAt >= startOfDay && d.CreatedAt <= endOfDay)
    .ToListAsync();
```

---

## ✅ Pre-Deployment Checklist

Before submitting your code, verify:

- [ ] No compilation errors or warnings
- [ ] All using statements cleaned up (remove unused)
- [ ] All TODO/HACK comments removed or addressed
- [ ] Proper error handling in all methods
- [ ] Logging added for important operations
- [ ] Authorization attributes on admin actions
- [ ] Anti-forgery tokens on all POST forms
- [ ] Input validation (Data Annotations + ModelState)
- [ ] Cancellation tokens passed through all async calls
- [ ] Soft delete used instead of hard delete
- [ ] Transactions used for multi-entity updates
- [ ] DTOs used instead of entities in views
- [ ] Navigation properties loaded with Include
- [ ] AsNoTracking used for read-only queries
- [ ] ViewBag repopulated on validation failures
- [ ] TempData used for success/error messages
- [ ] Audit fields set (CreatedAt, UpdatedAt)
- [ ] Decimal precision configured in EF Core
- [ ] UTC used for all timestamps
- [ ] Breadcrumb navigation added to views
- [ ] Bootstrap styling consistent with EstateFlow
- [ ] Icons added to buttons and headers
- [ ] Responsive design (Bootstrap grid)
- [ ] Confirmation modals for destructive actions

---

**Remember: If you're not sure, look at existing code first!**
