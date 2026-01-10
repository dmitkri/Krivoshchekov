using System.Text.RegularExpressions;
using FluentValidation;
using Tripwithfriends.DTO;

namespace Tripwithfriends.Validators;

public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    private static readonly Regex EmailRegex = new Regex(
        @"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public UpdateUserDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Имя не должно превышать 200 символов")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Email)
            .Must(email => !string.IsNullOrEmpty(email) && EmailRegex.IsMatch(email))
            .WithMessage("Неверный формат email. Пример: user@example.com")
            .MaximumLength(200).WithMessage("Email не должен превышать 200 символов")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Role)
            .Must(r => r == "Admin" || r == "Manager" || r == "User")
            .WithMessage("Role must be Admin, Manager, or User")
            .When(x => !string.IsNullOrEmpty(x.Role));
    }
}


