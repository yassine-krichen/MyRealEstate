using MyRealEstate.Application.Common.Models;
using MyRealEstate.Application.DTOs;
using MyRealEstate.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace MyRealEstate.Web.Models;

/// <summary>
/// ViewModel for public property listing page
/// </summary>
public class PublicPropertyListViewModel
{
    public PaginatedList<PropertyListDto> Properties { get; set; } = null!;
    public PropertySearchFilters SearchFilters { get; set; } = new();
}

/// <summary>
/// Search filters for property listing
/// </summary>
public class PropertySearchFilters
{
    public string? City { get; set; }
    public string? PropertyType { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinBedrooms { get; set; }
}

/// <summary>
/// ViewModel for public property details page with inquiry form
/// </summary>
public class PublicPropertyDetailViewModel
{
    public PropertyDetailDto Property { get; set; } = null!;
    public CreateInquiryViewModel InquiryForm { get; set; } = new();
}

/// <summary>
/// ViewModel for creating a new inquiry
/// </summary>
public class CreateInquiryViewModel
{
    public Guid? PropertyId { get; set; }
    
    [Required(ErrorMessage = "Your name is required.")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    [Display(Name = "Your Name")]
    public string VisitorName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Your email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters.")]
    [Display(Name = "Your Email")]
    public string VisitorEmail { get; set; } = string.Empty;
    
    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
    [Display(Name = "Your Phone (Optional)")]
    public string? VisitorPhone { get; set; }
    
    [Required(ErrorMessage = "Message is required.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Message must be between 10 and 2000 characters.")]
    [DataType(DataType.MultilineText)]
    [Display(Name = "Your Message")]
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel for inquiry tracking page
/// </summary>
public class InquiryTrackingViewModel
{
    public InquiryDetailDto Inquiry { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
    public bool CanReply => Inquiry.Status != InquiryStatus.Closed;
    public AddMessageViewModel ReplyForm { get; set; } = new();
}

/// <summary>
/// ViewModel for adding a message to an inquiry
/// </summary>
public class AddMessageViewModel
{
    [Required]
    [StringLength(32, MinimumLength = 32, ErrorMessage = "Invalid tracking token.")]
    public string Token { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Message is required.")]
    [StringLength(2000, MinimumLength = 1, ErrorMessage = "Message must be between 1 and 2000 characters.")]
    [DataType(DataType.MultilineText)]
    [Display(Name = "Your Reply")]
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel for inquiry created success page
/// </summary>
public class InquiryCreatedViewModel
{
    public string Token { get; set; } = string.Empty;
    public string TrackingUrl { get; set; } = string.Empty;
    public string VisitorEmail { get; set; } = string.Empty;
}
