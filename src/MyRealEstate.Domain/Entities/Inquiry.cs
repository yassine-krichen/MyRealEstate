using MyRealEstate.Domain.Enums;
using MyRealEstate.Domain.Interfaces;

namespace MyRealEstate.Domain.Entities;

public class Inquiry : BaseEntity, ISoftDelete
{
    public Guid? PropertyId { get; set; }
    public Property? Property { get; set; }
    
    public string VisitorName { get; set; } = string.Empty;
    public string VisitorEmail { get; set; } = string.Empty;
    public string? VisitorPhone { get; set; }
    public string InitialMessage { get; set; } = string.Empty;
    
    // Secure access token for visitors to track their inquiry
    public string AccessToken { get; set; } = string.Empty;
    
    public InquiryStatus Status { get; set; } = InquiryStatus.New;
    
    public Guid? AssignedAgentId { get; set; }
    public User? AssignedAgent { get; set; }
    
    public DateTime? ClosedAt { get; set; }
    public Guid? RelatedDealId { get; set; }
    
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    // Navigation
    public ICollection<ConversationMessage> Messages { get; set; } = new List<ConversationMessage>();

    // Business methods for status transitions
    public void AssignToAgent(Guid agentId)
    {
        if (Status == InquiryStatus.Closed)
            throw new InvalidOperationException("Cannot assign closed inquiries");
        
        AssignedAgentId = agentId;
        
        // Only change status to Assigned if it's New
        if (Status == InquiryStatus.New)
        {
            Status = InquiryStatus.Assigned;
        }
        // Keep InProgress status if already in progress
        
        UpdatedAt = DateTime.UtcNow;
    }

    public void StartProgress()
    {
        if (Status != InquiryStatus.Assigned && Status != InquiryStatus.New)
            throw new InvalidOperationException("Can only start progress on new or assigned inquiries");
        
        Status = InquiryStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsAnswered()
    {
        if (Status != InquiryStatus.InProgress && Status != InquiryStatus.Assigned)
            throw new InvalidOperationException("Can only mark as answered from InProgress or Assigned status");
        
        Status = InquiryStatus.Answered;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Close(Guid? dealId = null)
    {
        Status = InquiryStatus.Closed;
        ClosedAt = DateTime.UtcNow;
        RelatedDealId = dealId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
