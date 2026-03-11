using IlkProjem.Core.Dtos.SpecificationDtos;
using IlkProjem.Core.Models;
using IlkProjem.Core.Specifications;

public class CustomerCursorSpecification : BaseSpecification<Customer>
{
    public CustomerCursorSpecification(CustomerSpecParams custParams) 
        : base(x => 
            (string.IsNullOrEmpty(custParams.Search) || x.Name.ToLower().Contains(custParams.Search.ToLower())) &&
            (x.Id > custParams.LastId)
        )
    {
        AddOrderBy(x => x.Id);
        ApplyPaging(0, custParams.PageSize);
    }
}