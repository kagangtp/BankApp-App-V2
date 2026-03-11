using FluentValidation;
using IlkProjem.Core.Dtos.HouseDtos;

namespace IlkProjem.BLL.ValidationRules.FluentValidation.HouseDtoValidators;

public class HouseUpdateDtoValidator : AbstractValidator<HouseUpdateDto>
{
    public HouseUpdateDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Geçersiz ID değeri.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Adres zorunludur.")
            .MaximumLength(300).WithMessage("Adres en fazla 300 karakter olabilir.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir.");
    }
}
