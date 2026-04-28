using IlkProjem.Core.Models;
using IlkProjem.DAL.Data;
using IlkProjem.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IlkProjem.DAL.Repositories;

public class CarRepository : GenericRepository<Car>, ICarRepository
{
    public CarRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<Car>> GetByCustomerIdAsync(int customerId, CancellationToken ct = default)
    {
        return await _context.Cars
            .Where(c => c.CustomerId == customerId)
            .ToListAsync(ct);
    }
}
