using FluentValidation;

// T: Herhangi bir sınıf olabilir. 
// Ortak alanları valide etmek için kullanılır.
public class BaseValidator<T> : AbstractValidator<T>
{
    // Ortak kuralları buraya yazabilirsin.
    // Örneğin projedeki tüm DTO'ların bir Id'si varsa:
    protected void ValidateId(long id)
    {
        RuleFor(x => (long)x.GetType().GetProperty("Id").GetValue(x))
            .GreaterThan(0).WithMessage("Geçersiz ID değeri.");
    }
}