using IlkProjem.Core.Models;

namespace IlkProjem.DAL.Interfaces;

public interface ICarRepository : IGenericRepository<Car>
{
    Task<List<Car>> GetByCustomerIdAsync(int customerId, CancellationToken ct = default);
}
