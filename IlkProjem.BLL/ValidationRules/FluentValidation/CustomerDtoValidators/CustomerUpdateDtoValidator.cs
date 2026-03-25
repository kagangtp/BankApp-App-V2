using FluentValidation;
using IlkProjem.Core.Dtos.CustomerDtos;
using IlkProjem.Core.Utilities.Helpers;

namespace IlkProjem.BLL.ValidationRules.FluentValidation.CustomerDtoValidators;

public class CustomerUpdateDtoValidator : AbstractValidator<CustomerUpdateDto>
{
    public CustomerUpdateDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Güncellenecek ID belirtilmelidir.")
            .GreaterThan(0).WithMessage("Geçersiz ID değeri.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Müşteri adı ve soyadı boş olamaz.")
            .Length(2, 50).WithMessage("Ad Soyad 2 ile 50 karakter arasında olmalıdır.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Lütfen geçerli bir e-posta formatı giriniz.")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Balance)
            .GreaterThanOrEqualTo(0).WithMessage("Bakiye negatif olamaz.");

        RuleFor(x => x.TcKimlikNo)
            .Must(ValidationHelpers.BeValidTcKimlik)
            .When(x => !string.IsNullOrEmpty(x.TcKimlikNo))
            .WithMessage("Girilen TC Kimlik No geçerli değildir.");
    }
}
