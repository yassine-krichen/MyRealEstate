using FluentValidation;

namespace MyRealEstate.Application.Commands.Inquiries;

public class CreateInquiryCommandValidator : AbstractValidator<CreateInquiryCommand>
{
    public CreateInquiryCommandValidator()
    {
        RuleFor(x => x.PropertyId)
            .NotEmpty()
            .WithMessage("Property ID is required");

        RuleFor(x => x.ClientName)
            .NotEmpty()
            .WithMessage("Client name is required")
            .MaximumLength(100)
            .WithMessage("Client name must not exceed 100 characters");

        RuleFor(x => x.ClientEmail)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(256)
            .WithMessage("Email must not exceed 256 characters");

        RuleFor(x => x.ClientPhone)
            .MaximumLength(20)
            .WithMessage("Phone must not exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.ClientPhone));

        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("Message is required")
            .MinimumLength(10)
            .WithMessage("Message must be at least 10 characters")
            .MaximumLength(2000)
            .WithMessage("Message must not exceed 2000 characters");
    }
}

public class AssignInquiryCommandValidator : AbstractValidator<AssignInquiryCommand>
{
    public AssignInquiryCommandValidator()
    {
        RuleFor(x => x.InquiryId)
            .NotEmpty()
            .WithMessage("Inquiry ID is required");

        RuleFor(x => x.AgentId)
            .NotEmpty()
            .WithMessage("Agent ID is required");
    }
}

public class AddMessageCommandValidator : AbstractValidator<AddMessageCommand>
{
    public AddMessageCommandValidator()
    {
        RuleFor(x => x.InquiryId)
            .NotEmpty()
            .WithMessage("Inquiry ID is required");

        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("Message is required")
            .MinimumLength(1)
            .WithMessage("Message cannot be empty")
            .MaximumLength(2000)
            .WithMessage("Message must not exceed 2000 characters");

        RuleFor(x => x.SenderType)
            .IsInEnum()
            .WithMessage("Invalid sender type");
    }
}
