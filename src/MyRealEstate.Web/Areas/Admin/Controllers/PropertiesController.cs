using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyRealEstate.Application.Commands.Properties;
using MyRealEstate.Application.Queries.Properties;
using MyRealEstate.Domain.Enums;
using MyRealEstate.Web.Models;

namespace MyRealEstate.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class PropertiesController : Controller
{
    private readonly IMediator _mediator;
    private readonly ILogger<PropertiesController> _logger;
    
    public PropertiesController(IMediator mediator, ILogger<PropertiesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }
    
    // GET: Admin/Properties
    public async Task<IActionResult> Index(int page = 1, PropertyStatus? status = null)
    {
        var query = new GetPropertiesQuery
        {
            Page = page,
            PageSize = 20,
            Status = status
        };
        
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return View(new List<PropertyListViewModel>());
        }
        
        var viewModel = result.Value.Items.Select(dto => new PropertyListViewModel
        {
            Id = dto.Id,
            Title = dto.Title,
            Price = dto.Price,
            Currency = dto.Currency,
            PropertyType = dto.PropertyType,
            Bedrooms = dto.Bedrooms,
            Bathrooms = dto.Bathrooms,
            AreaSqM = dto.AreaSqM,
            City = dto.City,
            Status = dto.Status,
            MainImageUrl = dto.MainImageUrl,
            CreatedAt = dto.CreatedAt
        }).ToList();
        
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = result.Value.TotalPages;
        ViewBag.TotalCount = result.Value.TotalCount;
        ViewBag.StatusFilter = status;
        
        return View(viewModel);
    }
    
    // GET: Admin/Properties/Details/5
    public async Task<IActionResult> Details(Guid id)
    {
        var query = new GetPropertyByIdQuery(id);
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }
        
        var dto = result.Value;
        var viewModel = new PropertyDetailViewModel
        {
            Id = dto.Id,
            Title = dto.Title,
            Description = dto.Description,
            Price = dto.Price,
            Currency = dto.Currency,
            PropertyType = dto.PropertyType,
            Bedrooms = dto.Bedrooms,
            Bathrooms = dto.Bathrooms,
            AreaSqM = dto.AreaSqM,
            Status = dto.Status,
            Address = new AddressViewModel
            {
                Line1 = dto.Address.Line1,
                Line2 = dto.Address.Line2,
                City = dto.Address.City,
                State = dto.Address.State,
                PostalCode = dto.Address.PostalCode,
                Country = dto.Address.Country,
                Latitude = dto.Address.Latitude,
                Longitude = dto.Address.Longitude
            },
            Agent = dto.Agent != null ? new AgentViewModel
            {
                Id = dto.Agent.Id,
                FullName = dto.Agent.FullName,
                Email = dto.Agent.Email,
                PhoneNumber = dto.Agent.PhoneNumber
            } : null,
            Images = dto.Images.Select(img => new PropertyImageViewModel
            {
                Id = img.Id,
                Url = img.Url,
                FileName = img.FileName,
                IsMain = img.IsMain
            }).ToList(),
            ViewCount = dto.ViewCount,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
        
        return View(viewModel);
    }
    
    // GET: Admin/Properties/Create
    public IActionResult Create()
    {
        return View(new PropertyCreateViewModel());
    }
    
    // POST: Admin/Properties/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PropertyCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        var command = new CreatePropertyCommand
        {
            Title = model.Title,
            Description = model.Description,
            Price = model.Price,
            Currency = model.Currency,
            PropertyType = model.PropertyType,
            Bedrooms = model.Bedrooms,
            Bathrooms = model.Bathrooms,
            AreaSqM = model.AreaSqM,
            AddressLine1 = model.AddressLine1,
            AddressLine2 = model.AddressLine2,
            City = model.City,
            State = model.State,
            PostalCode = model.PostalCode,
            Country = model.Country,
            Latitude = model.Latitude,
            Longitude = model.Longitude,
            AgentId = model.AgentId
        };
        
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error);
            return View(model);
        }
        
        TempData["Success"] = "Property created successfully";
        return RedirectToAction(nameof(Edit), new { id = result.Value });
    }
    
    // GET: Admin/Properties/Edit/5
    public async Task<IActionResult> Edit(Guid id)
    {
        var query = new GetPropertyByIdQuery(id);
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }
        
        var dto = result.Value;
        var viewModel = new PropertyEditViewModel
        {
            Id = dto.Id,
            Title = dto.Title,
            Description = dto.Description,
            Price = dto.Price,
            Currency = dto.Currency,
            PropertyType = dto.PropertyType,
            Bedrooms = dto.Bedrooms,
            Bathrooms = dto.Bathrooms,
            AreaSqM = dto.AreaSqM,
            AddressLine1 = dto.Address.Line1,
            AddressLine2 = dto.Address.Line2,
            City = dto.Address.City,
            State = dto.Address.State,
            PostalCode = dto.Address.PostalCode,
            Country = dto.Address.Country,
            Latitude = dto.Address.Latitude,
            Longitude = dto.Address.Longitude,
            AgentId = dto.Agent?.Id,
            Status = dto.Status,
            ExistingImages = dto.Images.Select(img => new PropertyImageViewModel
            {
                Id = img.Id,
                Url = img.Url,
                FileName = img.FileName,
                IsMain = img.IsMain
            }).ToList()
        };
        
        return View(viewModel);
    }
    
    // POST: Admin/Properties/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, PropertyEditViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }
        
        if (!ModelState.IsValid)
        {
            // Reload images
            var queryForImages = new GetPropertyByIdQuery(id);
            var resultForImages = await _mediator.Send(queryForImages);
            if (resultForImages.IsSuccess)
            {
                model.ExistingImages = resultForImages.Value.Images.Select(img => new PropertyImageViewModel
                {
                    Id = img.Id,
                    Url = img.Url,
                    FileName = img.FileName,
                    IsMain = img.IsMain
                }).ToList();
            }
            return View(model);
        }
        
        var command = new UpdatePropertyCommand
        {
            Id = model.Id,
            Title = model.Title,
            Description = model.Description,
            Price = model.Price,
            Currency = model.Currency,
            PropertyType = model.PropertyType,
            Bedrooms = model.Bedrooms,
            Bathrooms = model.Bathrooms,
            AreaSqM = model.AreaSqM,
            AddressLine1 = model.AddressLine1,
            AddressLine2 = model.AddressLine2,
            City = model.City,
            State = model.State,
            PostalCode = model.PostalCode,
            Country = model.Country,
            Latitude = model.Latitude,
            Longitude = model.Longitude
        };
        
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error);
            return View(model);
        }
        
        TempData["Success"] = "Property updated successfully";
        return RedirectToAction(nameof(Edit), new { id = model.Id });
    }
    
    // POST: Admin/Properties/UploadImage/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile file, bool setAsMain = false)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Please select a file to upload";
            return RedirectToAction(nameof(Edit), new { id });
        }
        
        using var stream = file.OpenReadStream();
        var command = new UploadPropertyImageCommand
        {
            PropertyId = id,
            FileStream = stream,
            FileName = file.FileName,
            SetAsMain = setAsMain
        };
        
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Image uploaded successfully";
        }
        
        return RedirectToAction(nameof(Edit), new { id });
    }
    
    // POST: Admin/Properties/DeleteImage
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage(Guid imageId, Guid propertyId)
    {
        var command = new DeletePropertyImageCommand(imageId);
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Image deleted successfully";
        }
        
        return RedirectToAction(nameof(Edit), new { id = propertyId });
    }
    
    // POST: Admin/Properties/SetMainImage
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetMainImage(Guid imageId, Guid propertyId)
    {
        var command = new SetMainImageCommand(imageId);
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Main image set successfully";
        }
        
        return RedirectToAction(nameof(Edit), new { id = propertyId });
    }
    
    // POST: Admin/Properties/Publish/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(Guid id)
    {
        var command = new PublishPropertyCommand(id);
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Property published successfully";
        }
        
        return RedirectToAction(nameof(Edit), new { id });
    }
    
    // GET: Admin/Properties/Delete/5
    public async Task<IActionResult> Delete(Guid id)
    {
        var query = new GetPropertyByIdQuery(id);
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }
        
        var dto = result.Value;
        var viewModel = new PropertyDetailViewModel
        {
            Id = dto.Id,
            Title = dto.Title,
            Description = dto.Description,
            Price = dto.Price,
            Currency = dto.Currency,
            PropertyType = dto.PropertyType,
            Bedrooms = dto.Bedrooms,
            Bathrooms = dto.Bathrooms,
            AreaSqM = dto.AreaSqM,
            Status = dto.Status,
            Address = new AddressViewModel
            {
                Line1 = dto.Address.Line1,
                Line2 = dto.Address.Line2,
                City = dto.Address.City,
                State = dto.Address.State,
                PostalCode = dto.Address.PostalCode,
                Country = dto.Address.Country
            }
        };
        
        return View(viewModel);
    }
    
    // POST: Admin/Properties/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var command = new DeletePropertyCommand(id);
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Delete), new { id });
        }
        
        TempData["Success"] = "Property deleted successfully";
        return RedirectToAction(nameof(Index));
    }
}
