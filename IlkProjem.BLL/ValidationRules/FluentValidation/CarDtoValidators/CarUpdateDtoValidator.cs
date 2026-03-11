using FluentValidation;
using IlkProjem.Core.Dtos.CarDtos;

namespace IlkProjem.BLL.ValidationRules.FluentValidation.CarDtoValidators;

public class CarUpdateDtoValidator : AbstractValidator<CarUpdateDto>
{
    public CarUpdateDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Geçersiz ID değeri.");

        RuleFor(x => x.Plate)
            .NotEmpty().WithMessage("Plaka zorunludur.")
            .MaximumLength(20).WithMessage("Plaka en fazla 20 karakter olabilir.");

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("Açıklama en fazla 200 karakter olabilir.");
    }
}
