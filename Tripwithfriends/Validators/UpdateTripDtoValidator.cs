using FluentValidation;
using Tripwithfriends.DTO;

namespace Tripwithfriends.Validators;

public class UpdateTripDtoValidator : AbstractValidator<UpdateTripDto>
{
    public UpdateTripDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate!.Value)
            .WithMessage("End date must be after start date")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
    }
}

