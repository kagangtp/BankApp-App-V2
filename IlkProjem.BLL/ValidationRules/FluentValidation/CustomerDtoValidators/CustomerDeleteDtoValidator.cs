using FluentValidation;
using IlkProjem.Core.Dtos.CustomerDtos;

namespace IlkProjem.BLL.ValidationRules.FluentValidation.CustomerDtoValidators;

public class CustomerDeleteDtoValidator : AbstractValidator<CustomerDeleteDto>
{
    public CustomerDeleteDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Silinecek ID belirtilmelidir.")
            .GreaterThan(0).WithMessage("Geçersiz ID değeri.");
    }
}
