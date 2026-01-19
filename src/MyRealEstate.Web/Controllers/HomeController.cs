using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MyRealEstate.Web.Models;

namespace MyRealEstate.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
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
