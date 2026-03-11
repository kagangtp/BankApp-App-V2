using FluentValidation;
using IlkProjem.Core.Dtos.FileDtos;

namespace IlkProjem.BLL.ValidationRules.FluentValidation.FileDtoValidators;

public class FileAssignDtoValidator : AbstractValidator<FileAssignDto>
{
    private static readonly string[] AllowedOwnerTypes = { "Customer", "Car", "House" };

    public FileAssignDtoValidator()
    {
        RuleFor(x => x.OwnerId)
            .GreaterThan(0).WithMessage("Geçersiz sahip ID değeri.");

        RuleFor(x => x.OwnerType)
            .NotEmpty().WithMessage("Sahip tipi zorunludur.")
            .Must(type => AllowedOwnerTypes.Contains(type))
            .WithMessage("Sahip tipi yalnızca 'Customer', 'Car' veya 'House' olabilir.");
    }
}
