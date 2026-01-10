using System.Text.RegularExpressions;
using FluentValidation;
using Tripwithfriends.DTO;

namespace Tripwithfriends.Validators;

public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    private static readonly Regex EmailRegex = new Regex(
        @"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public CreateUserDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Имя обязательно для заполнения")
            .MaximumLength(200).WithMessage("Имя не должно превышать 200 символов");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email обязателен для заполнения")
            .Must(email => EmailRegex.IsMatch(email))
            .WithMessage("Неверный формат email. Пример: user@example.com")
            .MaximumLength(200).WithMessage("Email не должен превышать 200 символов");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен для заполнения")
            .MinimumLength(6).WithMessage("Пароль должен содержать минимум 6 символов");

        RuleFor(x => x.Role)
            .Must(r => r == "Admin" || r == "Manager" || r == "User")
            .WithMessage("Роль должна быть: Admin, Manager или User");
    }
}


