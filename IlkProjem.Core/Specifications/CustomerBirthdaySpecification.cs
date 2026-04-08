using IlkProjem.Core.Models;

namespace IlkProjem.Core.Specifications;

public class CustomerBirthdaySpecification : BaseSpecification<Customer>
{
    public CustomerBirthdaySpecification(DateTime date) 
        : base(x => x.BirthDate.HasValue && 
                    x.BirthDate.Value.Month == date.Month && 
                    x.BirthDate.Value.Day == date.Day)
    {
    }
}
