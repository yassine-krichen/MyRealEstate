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

    public ContentController(IMediator mediator, ILogger<ContentController> logger, IConfiguration configuration)
    {
        _mediator = mediator;
        _logger = logger;
        _configuration = configuration;
    }

    // GET: Admin/Content
    public async Task<IActionResult> Index()
    {
        var entries = await _mediator.Send(new GetAllContentEntriesQuery());
        return View(entries);
    }

    // GET: Admin/Content/TestApiKey - Diagnostic endpoint
    public IActionResult TestApiKey()
    {
        var apiKey = _configuration["TinyMCE:ApiKey"];
        var hasKey = !string.IsNullOrEmpty(apiKey);
        var keyLength = apiKey?.Length ?? 0;
        var preview = hasKey ? $"{apiKey?.Substring(0, Math.Min(8, keyLength))}..." : "NOT FOUND";
        
        return Content($@"
TinyMCE API Key Configuration Test
===================================
Has Key: {hasKey}
Key Length: {keyLength}
Preview: {preview}
UserSecretsId: aba299c4-83ef-497c-987b-ff86de3514c8

Expected Location: %APPDATA%\Microsoft\UserSecrets\aba299c4-83ef-497c-987b-ff86de3514c8\secrets.json

If 'Has Key' is False, run this command:
dotnet user-secrets set ""TinyMCE:ApiKey"" ""your-api-key"" --project src/MyRealEstate.Web
        ", "text/plain");
    }

    // GET: Admin/Content/Create
    public IActionResult Create()
    {
        var apiKey = _configuration["TinyMCE:ApiKey"];
        _logger.LogInformation("TinyMCE API Key loaded: {HasKey} (Length: {Length})", 
            !string.IsNullOrEmpty(apiKey), 
            apiKey?.Length ?? 0);
        
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
        _logger.LogInformation("TinyMCE API Key loaded for Edit: {HasKey} (Length: {Length})", 
            !string.IsNullOrEmpty(apiKey), 
            apiKey?.Length ?? 0);
        
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
