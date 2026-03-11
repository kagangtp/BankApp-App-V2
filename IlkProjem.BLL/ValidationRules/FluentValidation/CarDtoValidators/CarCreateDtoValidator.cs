using FluentValidation;
using IlkProjem.Core.Dtos.CarDtos;

namespace IlkProjem.BLL.ValidationRules.FluentValidation.CarDtoValidators;

public class CarCreateDtoValidator : AbstractValidator<CarCreateDto>
{
    public CarCreateDtoValidator()
    {
        RuleFor(x => x.Plate).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(200);
    }
}