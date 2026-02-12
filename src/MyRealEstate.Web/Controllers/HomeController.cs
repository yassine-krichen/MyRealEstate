using System.Diagnostics;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyRealEstate.Application.Queries.Content;
using MyRealEstate.Web.Models;

namespace MyRealEstate.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IMediator _mediator;

    public HomeController(ILogger<HomeController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public async Task<IActionResult> Index()
    {
        // Fetch HomeHero content with fallback
        var homeHeroContent = await _mediator.Send(new GetContentByKeyQuery("HomeHero"));
        ViewBag.HomeHero = homeHeroContent ?? 
            "<h1 class=\"display-3 fw-bold mb-3\">Find Your Dream Property</h1><p class=\"lead mb-4\">Discover exclusive real estate listings in Tunisia's most sought-after locations</p>";
        
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var model = new ErrorViewModel 
        { 
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
        };
        
        // Pass exception to view in development mode
        if (HttpContext.Items.TryGetValue("Exception", out var exception))
        {
            ViewData["Exception"] = exception;
        }
        
        if (HttpContext.Items.TryGetValue("ErrorMessage", out var errorMessage))
        {
            ViewData["ErrorMessage"] = errorMessage;
        }
        
        if (HttpContext.Items.TryGetValue("StatusCode", out var statusCode))
        {
            Response.StatusCode = (int)statusCode;
        }
        
        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult PageNotFound()
    {
        Response.StatusCode = 404;
        return View("NotFound");
    }
}
