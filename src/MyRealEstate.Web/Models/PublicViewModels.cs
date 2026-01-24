using MyRealEstate.Application.Common.Models;
using MyRealEstate.Application.DTOs;
using MyRealEstate.Domain.Enums;

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
    public string VisitorName { get; set; } = string.Empty;
    public string VisitorEmail { get; set; } = string.Empty;
    public string? VisitorPhone { get; set; }
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
    public string Token { get; set; } = string.Empty;
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
