using MyRealEstate.Domain.Enums;

namespace MyRealEstate.Domain.Entities;

public class Deal : BaseEntity
{
    public Guid PropertyId { get; set; }
    public Property Property { get; set; } = null!;
    
    public Guid? InquiryId { get; set; }
    public Inquiry? Inquiry { get; set; }
    
    public Guid AgentId { get; set; }
    public User Agent { get; set; } = null!;
    
    public decimal SalePrice { get; set; }
    public decimal? CommissionPercent { get; set; }
    // Store the calculated amount for reporting and accounting purposes
    public decimal? CommissionAmount { get; set; }
    
    public string? BuyerName { get; set; }
    public string? BuyerEmail { get; set; }
    public string? BuyerPhone { get; set; }
    public string? Notes { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DealStatus Status { get; set; } = DealStatus.Pending;

    // Business methods
    public void CalculateCommission()
    {
        if (CommissionPercent.HasValue && CommissionPercent.Value > 0)
        {
            CommissionAmount = SalePrice * (CommissionPercent.Value / 100);
        }
    }

    public void Complete()
    {
        if (Status != DealStatus.Pending)
            throw new InvalidOperationException("Only pending deals can be completed");
        
        Status = DealStatus.Completed;
        ClosedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == DealStatus.Completed)
            throw new InvalidOperationException("Completed deals cannot be cancelled");
        
        Status = DealStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}
