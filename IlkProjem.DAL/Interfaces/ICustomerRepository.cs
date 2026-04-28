// Interfaces/ICustomerRepository.cs
using IlkProjem.Core.Models;
using IlkProjem.Core.Specifications;

namespace IlkProjem.DAL.Interfaces;
public interface ICustomerRepository : IGenericRepository<Customer>
{
    // Specialized customer methods would go here
    Task<List<Customer>> GetAllAsync(CancellationToken ct = default); // Keep for compatibility if used elsewhere as GetAll
}