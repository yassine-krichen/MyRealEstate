using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyRealEstate.Application.Commands.Content;
using MyRealEstate.Application.Queries.Content;

namespace MyRealEstate.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ContentController : Controller
{
    private readonly IMediator _mediator;
    private readonly ILogger<ContentController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public ContentController(IMediator mediator, ILogger<ContentController> logger, IConfiguration configuration, IWebHostEnvironment environment)
    {
        _mediator = mediator;
        _logger = logger;
        _configuration = configuration;
        _environment = environment;
    }

    // GET: Admin/Content
    public async Task<IActionResult> Index()
    {
        var entries = await _mediator.Send(new GetAllContentEntriesQuery());
        return View(entries);
    }

    // GET: Admin/Content/TestApiKey - Diagnostic endpoint
    // Development only. It reports whether the key is configured, never any part of it.
    public IActionResult TestApiKey()
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        var hasKey = !string.IsNullOrEmpty(_configuration["TinyMCE:ApiKey"]);

        return Content($@"
TinyMCE API Key Configuration Test
===================================
Configured: {(hasKey ? "yes" : "no")}

If 'Configured' is no, run this command:
dotnet user-secrets set ""TinyMCE:ApiKey"" ""your-api-key"" --project src/MyRealEstate.Web
        ", "text/plain");
    }

    // GET: Admin/Content/Create
    public IActionResult Create()
    {
        var apiKey = _configuration["TinyMCE:ApiKey"];
        _logger.LogInformation("TinyMCE API Key loaded: {HasKey}", !string.IsNullOrEmpty(apiKey));
        
        ViewBag.TinyMceApiKey = apiKey ?? "no-api-key";
        return View();
    }

    // POST: Admin/Content/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateContentEntryCommand command)
    {
        if (!ModelState.IsValid)
        {
            return View(command);
        }

        try
        {
            await _mediator.Send(command);
            TempData["Success"] = "Content entry created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("Key", ex.Message);
            return View(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating content entry");
            TempData["Error"] = "Failed to create content entry.";
            return View(command);
        }
    }

    // GET: Admin/Content/Edit/5
    public async Task<IActionResult> Edit(Guid id)
    {
        var entries = await _mediator.Send(new GetAllContentEntriesQuery());
        var entry = entries.FirstOrDefault(e => e.Id == id);

        if (entry == null)
        {
            return NotFound();
        }

        var command = new UpdateContentEntryCommand
        {
            Id = entry.Id,
            Key = entry.Key,
            HtmlValue = entry.HtmlValue
        };

        var apiKey = _configuration["TinyMCE:ApiKey"];
        _logger.LogInformation("TinyMCE API Key loaded for Edit: {HasKey}", !string.IsNullOrEmpty(apiKey));
        
        ViewBag.TinyMceApiKey = apiKey ?? "no-api-key";
        return View(command);
    }

    // POST: Admin/Content/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateContentEntryCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(command);
        }

        try
        {
            await _mediator.Send(command);
            TempData["Success"] = "Content entry updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("Key", ex.Message);
            return View(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating content entry");
            TempData["Error"] = "Failed to update content entry.";
            ViewBag.TinyMceApiKey = _configuration["TinyMCE:ApiKey"] ?? "no-api-key";
            return View(command);
        }
    }

    // POST: Admin/Content/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _mediator.Send(new DeleteContentEntryCommand { Id = id });
            TempData["Success"] = "Content entry deleted successfully.";
        }
        catch (KeyNotFoundException)
        {
            TempData["Error"] = "Content entry not found.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting content entry");
            TempData["Error"] = "Failed to delete content entry.";
        }

        return RedirectToAction(nameof(Index));
    }
}
