using IlkProjem.Core.Models;

namespace IlkProjem.DAL.Interfaces;
public interface IHouseRepository : IGenericRepository<House>
{
    Task<List<House>> GetByCustomerIdAsync(int customerId, CancellationToken ct = default);
}
