using MyRealEstate.Domain.Enums;

namespace MyRealEstate.Domain.Entities;

public class ConversationMessage : BaseEntity
{
    public Guid InquiryId { get; set; }
    public Inquiry Inquiry { get; set; } = null!;
    
    public SenderType SenderType { get; set; }
    public Guid? SenderUserId { get; set; }
    public User? SenderUser { get; set; }
    
    public string Body { get; set; } = string.Empty;
    // Used for private comments, reminders, or instructions about an inquiry that should not be shown to the customer.
    public bool IsInternalNote { get; set; }
}
