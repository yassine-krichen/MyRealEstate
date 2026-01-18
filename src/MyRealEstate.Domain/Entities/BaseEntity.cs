using MyRealEstate.Domain.Interfaces;

namespace MyRealEstate.Domain.Entities;

public abstract class BaseEntity : IAuditable
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
