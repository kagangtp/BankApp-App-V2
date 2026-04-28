using IlkProjem.Core.Models;
using IlkProjem.DAL.Data;
using IlkProjem.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IlkProjem.DAL.Repositories;

public class HouseRepository : GenericRepository<House>, IHouseRepository
{
    public HouseRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<House>> GetByCustomerIdAsync(int customerId, CancellationToken ct = default)
    {
        return await _context.Houses
            .Where(h => h.CustomerId == customerId)
            .ToListAsync(ct);
    }
}
