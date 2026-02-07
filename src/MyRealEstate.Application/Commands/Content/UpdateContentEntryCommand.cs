using MediatR;
using System.ComponentModel.DataAnnotations;

namespace MyRealEstate.Application.Commands.Content;

/// <summary>
/// Command to update an existing content entry
/// </summary>
public class UpdateContentEntryCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
    
    [Required(ErrorMessage = "Content key is required.")]
    [StringLength(100, ErrorMessage = "Key cannot exceed 100 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Key can only contain letters, numbers, and underscores.")]
    public string Key { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Content value is required.")]
    public string HtmlValue { get; set; } = string.Empty;
}
