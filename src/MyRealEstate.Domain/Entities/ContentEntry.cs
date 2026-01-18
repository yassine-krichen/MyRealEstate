namespace MyRealEstate.Domain.Entities;

// for storing editable site content (like About page text, homepage hero, 
// email templates) in the database, so admins can update it without a code deploy
public class ContentEntry : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string HtmlValue { get; set; } = string.Empty;
    public Guid? UpdatedByUserId { get; set; }
    public User? UpdatedByUser { get; set; }
}
