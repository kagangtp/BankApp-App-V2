using IlkProjem.Core.Dtos.CustomerDtos;
using IlkProjem.Core.Dtos.SpecificationDtos;
using IlkProjem.Core.Utilities.Results;

namespace IlkProjem.BLL.Interfaces;

public interface ICustomerService
{
    Task<IResult> AddCustomer(CustomerCreateDto createDto, CancellationToken ct = default);
    Task<IDataResult<List<CustomerReadDto>>> GetCustomersAsync(CustomerSpecParams custParams, CancellationToken ct = default);
    Task<IDataResult<CustomerReadDto>> GetCustomerById(int id, CancellationToken ct = default);
    Task<IResult> UpdateCustomer(CustomerUpdateDto updateDto, CancellationToken ct = default);
    Task<IResult> DeleteCustomer(CustomerDeleteDto deleteDto, CancellationToken ct = default);
}