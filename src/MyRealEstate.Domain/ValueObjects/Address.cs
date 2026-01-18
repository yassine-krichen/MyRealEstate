namespace MyRealEstate.Domain.ValueObjects;

public class Address
{
    public string Line1 { get; private set; } = string.Empty;
    public string? Line2 { get; private set; }
    public string City { get; private set; } = string.Empty;
    public string? State { get; private set; }
    public string? PostalCode { get; private set; }
    public string Country { get; private set; } = "Tunisia";
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }

    private Address() { } // For EF Core

    public Address(
        string line1,
        string city,
        string country = "Tunisia",
        string? line2 = null,
        string? state = null,
        string? postalCode = null,
        decimal? latitude = null,
        decimal? longitude = null)
    {
        if (string.IsNullOrWhiteSpace(line1))
            throw new ArgumentException("Address line 1 is required", nameof(line1));
        
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required", nameof(city));

        Line1 = line1;
        Line2 = line2;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
        Latitude = latitude;
        Longitude = longitude;
    }

    public string GetFullAddress()
    {
        var parts = new List<string> { Line1 };
        
        if (!string.IsNullOrWhiteSpace(Line2))
            parts.Add(Line2);
        
        parts.Add(City);
        
        if (!string.IsNullOrWhiteSpace(State))
            parts.Add(State);
        
        if (!string.IsNullOrWhiteSpace(PostalCode))
            parts.Add(PostalCode);
        
        parts.Add(Country);

        return string.Join(", ", parts);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Address other)
            return false;

        return Line1 == other.Line1 
            && Line2 == other.Line2 
            && City == other.City 
            && State == other.State 
            && PostalCode == other.PostalCode 
            && Country == other.Country;
    }

    public override int GetHashCode() => HashCode.Combine(Line1, Line2, City, State, PostalCode, Country);

    public override string ToString() => GetFullAddress();
}
