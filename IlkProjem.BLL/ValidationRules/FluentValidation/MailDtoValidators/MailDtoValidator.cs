using FluentValidation;
using IlkProjem.Core.Dtos.MailDtos;

namespace IlkProjem.BLL.ValidationRules.FluentValidation.MailDtoValidators;

public class MailDtoValidator : AbstractValidator<MailDto>
{
    public MailDtoValidator()
    {
        RuleFor(x => x.To)
            .NotEmpty().WithMessage("Alıcı e-posta adresi zorunludur.")
            .EmailAddress().WithMessage("Lütfen geçerli bir alıcı e-posta formatı giriniz.");

        RuleFor(x => x.Subject)
            .MaximumLength(200).WithMessage("Konu en fazla 200 karakter olabilir.");

        RuleFor(x => x.Body)
            .MaximumLength(5000).WithMessage("İçerik en fazla 5000 karakter olabilir.");
    }
}
