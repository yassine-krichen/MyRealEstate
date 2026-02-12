using MyRealEstate.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace MyRealEstate.Web.Models;

public class DealListViewModel
{
    public Guid Id { get; set; }
    public string PropertyTitle { get; set; } = string.Empty;
    public string? PropertyCity { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public string? BuyerEmail { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public decimal SalePrice { get; set; }
    public decimal? CommissionAmount { get; set; }
    public DealStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
}

public class DealDetailViewModel
{
    public Guid Id { get; set; }
    
    // Property info
    public Guid PropertyId { get; set; }
    public string PropertyTitle { get; set; } = string.Empty;
    public string? PropertyCity { get; set; }
    public string? PropertyMainImageUrl { get; set; }
    
    // Agent info
    public Guid AgentId { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public string? AgentEmail { get; set; }
    public string? AgentPhone { get; set; }
    
    // Inquiry info
    public Guid? InquiryId { get; set; }
    public string? InquiryVisitorName { get; set; }
    
    // Buyer info
    public string? BuyerName { get; set; }
    public string? BuyerEmail { get; set; }
    public string? BuyerPhone { get; set; }
    
    // Financial
    public decimal SalePrice { get; set; }
    public decimal? CommissionPercent { get; set; }
    public decimal? CommissionAmount { get; set; }
    
    // Status
    public DealStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
}

public class DealCreateViewModel
{
    [Required(ErrorMessage = "Property is required")]
    [Display(Name = "Property")]
    public Guid PropertyId { get; set; }

    [Display(Name = "Inquiry")]
    public Guid? InquiryId { get; set; }

    [Required(ErrorMessage = "Buyer name is required")]
    [StringLength(200, ErrorMessage = "Buyer name must not exceed 200 characters")]
    [Display(Name = "Buyer Name")]
    public string BuyerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Buyer email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(255, ErrorMessage = "Email must not exceed 255 characters")]
    [Display(Name = "Buyer Email")]
    public string BuyerEmail { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Phone must not exceed 50 characters")]
    [Display(Name = "Buyer Phone")]
    public string? BuyerPhone { get; set; }

    [Required(ErrorMessage = "Sale price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Sale price must be greater than 0")]
    [Display(Name = "Sale Price")]
    public decimal SalePrice { get; set; }

    [Required(ErrorMessage = "Commission rate is required")]
    [Range(0, 100, ErrorMessage = "Commission rate must be between 0 and 100")]
    [Display(Name = "Commission Rate (%)")]
    public decimal CommissionRate { get; set; } = 5.0m;

    [StringLength(2000, ErrorMessage = "Notes must not exceed 2000 characters")]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }
}

public class DealEditViewModel
{
    public Guid Id { get; set; }
    public DealStatus Status { get; set; }
    public string PropertyTitle { get; set; } = string.Empty;

    [Required(ErrorMessage = "Buyer name is required")]
    [StringLength(200, ErrorMessage = "Buyer name must not exceed 200 characters")]
    [Display(Name = "Buyer Name")]
    public string BuyerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Buyer email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(255, ErrorMessage = "Email must not exceed 255 characters")]
    [Display(Name = "Buyer Email")]
    public string BuyerEmail { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Phone must not exceed 50 characters")]
    [Display(Name = "Buyer Phone")]
    public string? BuyerPhone { get; set; }

    [Required(ErrorMessage = "Sale price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Sale price must be greater than 0")]
    [Display(Name = "Sale Price")]
    public decimal SalePrice { get; set; }

    [Required(ErrorMessage = "Commission rate is required")]
    [Range(0, 100, ErrorMessage = "Commission rate must be between 0 and 100")]
    [Display(Name = "Commission Rate (%)")]
    public decimal CommissionRate { get; set; } = 5.0m;

    [StringLength(2000, ErrorMessage = "Notes must not exceed 2000 characters")]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }
}

public class DealSearchViewModel
{
    public DealStatus? Status { get; set; }
    public Guid? AgentId { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public List<DealListViewModel> Results { get; set; } = new();
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
