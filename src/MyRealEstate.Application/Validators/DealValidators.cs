using FluentValidation;
using MyRealEstate.Application.Commands.Deals;

namespace MyRealEstate.Application.Validators;

public class CreateDealCommandValidator : AbstractValidator<CreateDealCommand>
{
    public CreateDealCommandValidator()
    {
        RuleFor(x => x.PropertyId)
            .NotEmpty()
            .WithMessage("Property is required");

        RuleFor(x => x.AgentId)
            .NotEmpty()
            .WithMessage("Agent is required");

        RuleFor(x => x.BuyerName)
            .NotEmpty()
            .WithMessage("Buyer name is required")
            .MaximumLength(200)
            .WithMessage("Buyer name must not exceed 200 characters");

        RuleFor(x => x.BuyerEmail)
            .NotEmpty()
            .WithMessage("Buyer email is required")
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(255)
            .WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.BuyerPhone)
            .MaximumLength(50)
            .WithMessage("Phone must not exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.BuyerPhone));

        RuleFor(x => x.SalePrice)
            .GreaterThan(0)
            .WithMessage("Sale price must be greater than 0");

        RuleFor(x => x.CommissionRate)
            .InclusiveBetween(0, 100)
            .WithMessage("Commission rate must be between 0 and 100");

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .WithMessage("Notes must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

public class UpdateDealCommandValidator : AbstractValidator<UpdateDealCommand>
{
    public UpdateDealCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Deal ID is required");

        RuleFor(x => x.BuyerName)
            .NotEmpty()
            .WithMessage("Buyer name is required")
            .MaximumLength(200)
            .WithMessage("Buyer name must not exceed 200 characters");

        RuleFor(x => x.BuyerEmail)
            .NotEmpty()
            .WithMessage("Buyer email is required")
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(255)
            .WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.BuyerPhone)
            .MaximumLength(50)
            .WithMessage("Phone must not exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.BuyerPhone));

        RuleFor(x => x.SalePrice)
            .GreaterThan(0)
            .WithMessage("Sale price must be greater than 0");

        RuleFor(x => x.CommissionRate)
            .InclusiveBetween(0, 100)
            .WithMessage("Commission rate must be between 0 and 100");

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .WithMessage("Notes must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
