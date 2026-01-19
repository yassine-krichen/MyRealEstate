using MyRealEstate.Domain.Enums;

namespace MyRealEstate.Application.DTOs;

public class InquiryDto
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
    public int MessageCount { get; set; }
}

public class InquiryDetailDto : InquiryDto
{
    public List<MessageDto> Messages { get; set; } = new();
}

public class MessageDto
{
    public Guid Id { get; set; }
    public Guid InquiryId { get; set; }
    public SenderType SenderType { get; set; }
    public string? SenderName { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}

public class CreateInquiryDto
{
    public Guid PropertyId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public string? ClientPhone { get; set; }
    public string Message { get; set; } = string.Empty;
}
