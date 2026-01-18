namespace MyRealEstate.Application.DTOs;

public class PropertyListDto
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

public class PropertyDetailDto
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
    
    public AddressDto Address { get; set; } = new();
    public AgentDto? Agent { get; set; }
    public List<PropertyImageDto> Images { get; set; } = new();
    
    public int ViewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class AddressDto
{
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}

public class PropertyImageDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public bool IsMain { get; set; }
}

public class AgentDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}
