using System.Net;
using System.Text.Json;
using MyRealEstate.Application.Common.Exceptions;

namespace MyRealEstate.Web.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;
    
    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred. TraceId: {TraceId}", context.TraceIdentifier);
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        
        // Check if this is an AJAX/API request (wants JSON) or a browser request (wants HTML)
        var isAjaxRequest = context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                           context.Request.Headers["Accept"].ToString().Contains("application/json");
        
        var (statusCode, message, errors) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                "Validation failed",
                validationEx.Errors
            ),
            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                notFoundEx.Message,
                null
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                _environment.IsDevelopment() ? exception.Message : "An error occurred while processing your request",
                null
            )
        };
        
        response.StatusCode = (int)statusCode;
        
        if (isAjaxRequest)
        {
            // Return JSON for AJAX requests
            response.ContentType = "application/json";
            var result = JsonSerializer.Serialize(new
            {
                success = false,
                message,
                errors,
                traceId = context.TraceIdentifier
            });
            await response.WriteAsync(result);
        }
        else
        {
            // Redirect to error page for browser requests
            var errorPath = statusCode switch
            {
                HttpStatusCode.NotFound => "/Home/NotFound",
                _ => "/Home/Error"
            };
            
            context.Items["ErrorMessage"] = message;
            context.Items["TraceId"] = context.TraceIdentifier;
            
            context.Response.Redirect(errorPath);
        }
    }
}
