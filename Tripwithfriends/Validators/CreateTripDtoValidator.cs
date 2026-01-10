using FluentValidation;
using Tripwithfriends.DTO;

namespace Tripwithfriends.Validators;

public class CreateTripDtoValidator : AbstractValidator<CreateTripDto>
{
    public CreateTripDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");
    }
}


