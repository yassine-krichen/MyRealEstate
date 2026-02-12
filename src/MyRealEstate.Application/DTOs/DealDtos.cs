using MyRealEstate.Domain.Enums;

namespace MyRealEstate.Application.DTOs;

public class DealListDto
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

public class DealDetailDto
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

public class DealStatisticsDto
{
    public int TotalDeals { get; set; }
    public int CompletedDeals { get; set; }
    public int PendingDeals { get; set; }
    public int CancelledDeals { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCommission { get; set; }
    public decimal AverageSalePrice { get; set; }
    public decimal AverageCommission { get; set; }
}
