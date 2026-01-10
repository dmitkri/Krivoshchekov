using System.Text.RegularExpressions;
using FluentValidation;
using Tripwithfriends.DTO;

namespace Tripwithfriends.Validators;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    private static readonly Regex EmailRegex = new Regex(
        @"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public LoginDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email обязателен для заполнения")
            .Must(email => EmailRegex.IsMatch(email))
            .WithMessage("Неверный формат email. Пример: user@example.com");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}


