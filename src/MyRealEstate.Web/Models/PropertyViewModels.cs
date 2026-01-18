using System.ComponentModel.DataAnnotations;

namespace MyRealEstate.Web.Models;

public class PropertyListViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PropertyType { get; set; } = string.Empty;
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public decimal AreaSqM { get; set; }
    public string City { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? MainImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PropertyDetailViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PropertyType { get; set; } = string.Empty;
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public decimal AreaSqM { get; set; }
    public string Status { get; set; } = string.Empty;
    
    public AddressViewModel Address { get; set; } = new();
    public AgentViewModel? Agent { get; set; }
    public List<PropertyImageViewModel> Images { get; set; } = new();
    
    public int ViewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class PropertyCreateViewModel
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title must not exceed 200 characters")]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Description is required")]
    [StringLength(5000, ErrorMessage = "Description must not exceed 5000 characters")]
    [DataType(DataType.MultilineText)]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Price is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Price must be 0 or greater")]
    [Display(Name = "Price")]
    public decimal Price { get; set; }
    
    [Required(ErrorMessage = "Currency is required")]
    [StringLength(3, ErrorMessage = "Currency must be 3 characters")]
    [Display(Name = "Currency")]
    public string Currency { get; set; } = "TND";
    
    [Required(ErrorMessage = "Property type is required")]
    [StringLength(50)]
    [Display(Name = "Property Type")]
    public string PropertyType { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Number of bedrooms is required")]
    [Range(0, 100, ErrorMessage = "Bedrooms must be between 0 and 100")]
    [Display(Name = "Bedrooms")]
    public int Bedrooms { get; set; }
    
    [Required(ErrorMessage = "Number of bathrooms is required")]
    [Range(0, 100, ErrorMessage = "Bathrooms must be between 0 and 100")]
    [Display(Name = "Bathrooms")]
    public int Bathrooms { get; set; }
    
    [Required(ErrorMessage = "Area is required")]
    [Range(1, double.MaxValue, ErrorMessage = "Area must be greater than 0")]
    [Display(Name = "Area (sq m)")]
    public decimal AreaSqM { get; set; }
    
    [Required(ErrorMessage = "Address line 1 is required")]
    [StringLength(200)]
    [Display(Name = "Address Line 1")]
    public string AddressLine1 { get; set; } = string.Empty;
    
    [StringLength(200)]
    [Display(Name = "Address Line 2")]
    public string? AddressLine2 { get; set; }
    
    [Required(ErrorMessage = "City is required")]
    [StringLength(100)]
    [Display(Name = "City")]
    public string City { get; set; } = string.Empty;
    
    [StringLength(100)]
    [Display(Name = "State/Province")]
    public string? State { get; set; }
    
    [Required(ErrorMessage = "Postal code is required")]
    [StringLength(20)]
    [Display(Name = "Postal Code")]
    public string PostalCode { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Country is required")]
    [StringLength(100)]
    [Display(Name = "Country")]
    public string Country { get; set; } = "Tunisia";
    
    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
    [Display(Name = "Latitude")]
    public decimal? Latitude { get; set; }
    
    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
    [Display(Name = "Longitude")]
    public decimal? Longitude { get; set; }
    
    [Display(Name = "Agent")]
    public Guid? AgentId { get; set; }
}

public class PropertyEditViewModel : PropertyCreateViewModel
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<PropertyImageViewModel> ExistingImages { get; set; } = new();
}

public class PropertyImageViewModel
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public bool IsMain { get; set; }
}

public class AddressViewModel
{
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    
    public string GetFullAddress()
    {
        var parts = new List<string> { Line1 };
        if (!string.IsNullOrWhiteSpace(Line2))
            parts.Add(Line2);
        parts.Add(City);
        if (!string.IsNullOrWhiteSpace(State))
            parts.Add(State);
        parts.Add(PostalCode);
        parts.Add(Country);
        return string.Join(", ", parts);
    }
}

public class AgentViewModel
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}

// For search/filter form
public class PropertySearchViewModel
{
    [Display(Name = "Search")]
    public string? SearchTerm { get; set; }
    
    [Display(Name = "Min Price")]
    public decimal? MinPrice { get; set; }
    
    [Display(Name = "Max Price")]
    public decimal? MaxPrice { get; set; }
    
    [Display(Name = "Min Bedrooms")]
    public int? MinBedrooms { get; set; }
    
    [Display(Name = "Max Bedrooms")]
    public int? MaxBedrooms { get; set; }
    
    [Display(Name = "Property Type")]
    public string? PropertyType { get; set; }
    
    [Display(Name = "City")]
    public string? City { get; set; }
    
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    
    public List<PropertyListViewModel> Results { get; set; } = new();
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
