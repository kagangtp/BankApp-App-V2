using FluentValidation;
using IlkProjem.Core.Dtos.UserDtos;

namespace IlkProjem.BLL.ValidationRules.FluentValidation.UserDtoValidators;

public class UserUpdateDtoValidator : AbstractValidator<UserUpdateDto>
{
    public UserUpdateDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username cannot be empty")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email cannot be empty")
            .EmailAddress().WithMessage("A valid email is required");
    }
}
