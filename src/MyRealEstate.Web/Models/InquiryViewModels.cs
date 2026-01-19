using MyRealEstate.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace MyRealEstate.Web.Models;

public class InquiryListViewModel
{
    public Guid Id { get; set; }
    public string PropertyTitle { get; set; } = string.Empty;
    public string? PropertyCity { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public InquiryStatus Status { get; set; }
    public string? AssignedToName { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MessageCount { get; set; }
}

public class InquiryDetailViewModel
{
    public Guid Id { get; set; }
    public Guid PropertyId { get; set; }
    public string PropertyTitle { get; set; } = string.Empty;
    public string? PropertyCity { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public string? ClientPhone { get; set; }
    public string Message { get; set; } = string.Empty;
    public InquiryStatus Status { get; set; }
    public Guid? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public List<MessageViewModel> Messages { get; set; } = new();
}

public class MessageViewModel
{
    public Guid Id { get; set; }
    public SenderType SenderType { get; set; }
    public string? SenderName { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}

public class InquiryCreateViewModel
{
    public Guid PropertyId { get; set; }
    public string PropertyTitle { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name must not exceed 100 characters")]
    [Display(Name = "Your Name")]
    public string ClientName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(256, ErrorMessage = "Email must not exceed 256 characters")]
    [Display(Name = "Your Email")]
    public string ClientEmail { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "Phone must not exceed 20 characters")]
    [Display(Name = "Phone Number (Optional)")]
    public string? ClientPhone { get; set; }

    [Required(ErrorMessage = "Message is required")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Message must be between 10 and 2000 characters")]
    [Display(Name = "Your Message")]
    public string Message { get; set; } = string.Empty;
}

public class ReplyToInquiryViewModel
{
    public Guid InquiryId { get; set; }

    [Required(ErrorMessage = "Message is required")]
    [StringLength(2000, ErrorMessage = "Message must not exceed 2000 characters")]
    [Display(Name = "Your Reply")]
    public string Message { get; set; } = string.Empty;
}

public class AssignInquiryViewModel
{
    public Guid InquiryId { get; set; }

    [Required(ErrorMessage = "Agent is required")]
    [Display(Name = "Assign to Agent")]
    public Guid AgentId { get; set; }
}

public class InquirySearchViewModel
{
    public InquiryStatus? Status { get; set; }
    public Guid? AssignedToId { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public List<InquiryListViewModel> Results { get; set; } = new();
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
