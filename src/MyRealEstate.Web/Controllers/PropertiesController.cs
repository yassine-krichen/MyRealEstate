using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyRealEstate.Application.Queries.Properties;
using MyRealEstate.Domain.Enums;
using MyRealEstate.Web.Models;

namespace MyRealEstate.Web.Controllers;

public class PropertiesController : Controller
{
    private readonly IMediator _mediator;

    public PropertiesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET: /Properties
    public async Task<IActionResult> Index(int page = 1, string? city = null, string? propertyType = null, decimal? minPrice = null, decimal? maxPrice = null, int? minBedrooms = null)
    {
        var query = new GetPropertiesQuery
        {
            Page = page,
            PageSize = 12,
            Status = PropertyStatus.Published, // Only show published properties
            City = city,
            PropertyType = propertyType,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            MinBedrooms = minBedrooms
        };

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return View("Error");
        }

        var viewModel = new PublicPropertyListViewModel
        {
            Properties = result.Value,
            SearchFilters = new PropertySearchFilters
            {
                City = city,
                PropertyType = propertyType,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                MinBedrooms = minBedrooms
            }
        };

        return View(viewModel);
    }

    // GET: /Properties/Details/5
    public async Task<IActionResult> Details(Guid id)
    {
        var query = new GetPropertyByIdQuery(id, TrackView: true);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound();
        }

        // Only show published properties to public
        if (result.Value.Status != PropertyStatus.Published.ToString())
        {
            return NotFound();
        }

        var viewModel = new PublicPropertyDetailViewModel
        {
            Property = result.Value,
            InquiryForm = new CreateInquiryViewModel
            {
                PropertyId = id
            }
        };

        return View(viewModel);
    }
}
