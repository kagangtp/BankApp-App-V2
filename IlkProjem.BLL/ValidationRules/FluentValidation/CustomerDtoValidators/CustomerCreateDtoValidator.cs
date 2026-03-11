using FluentValidation;
using IlkProjem.Core.Dtos.CustomerDtos; // DTO'ların bulunduğu yer

namespace IlkProjem.BLL.ValidationRules.FluentValidation.CustomerDtoValidators;
public class CustomerCreateDtoValidator : AbstractValidator<CustomerCreateDto>
{
    public CustomerCreateDtoValidator()
    {
        // Ad Soyad: 2-50 karakter arası, boş olamaz
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Müşteri adı ve soyadı boş olamaz.")
            .Length(2, 50).WithMessage("Ad Soyad 2 ile 50 karakter arasında olmalıdır.");

        // E-posta: Geçerli formatta olmalı
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta adresi zorunludur.")
            .EmailAddress().WithMessage("Lütfen geçerli bir e-posta formatı giriniz.");

        // Yaş: 18'den küçük olamaz
        // Eğer DTO'da yaş alanı varsa:
        // RuleFor(x => x.MusteriYasi).GreaterThanOrEqualTo(18).WithMessage("18 yaş altı kayıt yapılamaz.");
    }
}