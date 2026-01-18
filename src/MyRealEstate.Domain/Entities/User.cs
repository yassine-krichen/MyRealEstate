using Microsoft.AspNetCore.Identity;
using MyRealEstate.Domain.Interfaces;

namespace MyRealEstate.Domain.Entities;

public class User : IdentityUser<Guid>, IAuditable, ISoftDelete
{
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
